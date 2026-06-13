using DTO.RabbitMq.Messages;
using Microsoft.EntityFrameworkCore;
using Placanja.API.Data;
using Placanja.API.Models;
using Placanja.API.Services;
using System.Text.Json;

namespace Placanja.API.HostedServices
{
    public class SagaChoreographyConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public SagaChoreographyConsumer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var bus = new RabbitMqBus();

            await bus.Subscribe<MestoRezervisano>("mesto-rezervisano-ch", async (evt) =>
            {
                Console.WriteLine($"[CHOREOGRAPHY] [PLACANJA] Received MestoRezervisano. CorrelationID: {evt.CorrelationID}, UcesnikID: {evt.UcesnikID}, Amount: {evt.CenaKotizacije}");
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<PlacanjaContext>();

                    var racun = await db.RacuniUcesnika
                        .FirstOrDefaultAsync(r => r.UcesnikID == evt.UcesnikID);

                    if (racun == null)
                    {
                        Console.WriteLine($"[CHOREOGRAPHY] [PLACANJA] Creating account for participant ID {evt.UcesnikID} with 10000.00 balance.");
                        racun = new RacunUcesnika
                        {
                            UcesnikID = evt.UcesnikID,
                            Ime = "Korisnik",
                            Prezime = "Choreography",
                            Email = $"ucesnik{evt.UcesnikID}@choreography.com",
                            StanjeNaRacunu = 10000.00
                        };
                        db.RacuniUcesnika.Add(racun);
                        await db.SaveChangesAsync();
                    }

                    if (racun.StanjeNaRacunu >= evt.CenaKotizacije)
                    {
                        racun.StanjeNaRacunu -= evt.CenaKotizacije;
                        db.RacuniUcesnika.Update(racun);
                        await db.SaveChangesAsync();

                        Console.WriteLine($"[CHOREOGRAPHY] [PLACANJA] Payment successful! Skinuto: {evt.CenaKotizacije}, New balance: {racun.StanjeNaRacunu}");

                        var naplaceno = new NaplacenaKotizacija
                        {
                            CorrelationID = evt.CorrelationID
                        };
                        await bus.Publish("kotizacija-naplacena-ch", JsonSerializer.Serialize(naplaceno));
                    }
                    else
                    {
                        Console.WriteLine($"[CHOREOGRAPHY] [PLACANJA] Low balance! Stanje: {racun.StanjeNaRacunu}, Amount: {evt.CenaKotizacije}");
                        var odbijeno = new OdbijenaKotizacija
                        {
                            CorrelationID = evt.CorrelationID,
                            StrucniDogadjajID = evt.StrucniDogadjajID,
                            Razlog = "Nedovoljno sredstava na racunu."
                        };
                        await bus.Publish("kotizacija-odbijena-events-ch", JsonSerializer.Serialize(odbijeno));
                        await bus.Publish("kotizacija-odbijena-prijave-ch", JsonSerializer.Serialize(odbijeno));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CHOREOGRAPHY] [PLACANJA ERROR] Error processing payment: {ex.Message}");
                    var odbijeno = new OdbijenaKotizacija
                    {
                        CorrelationID = evt.CorrelationID,
                        StrucniDogadjajID = evt.StrucniDogadjajID,
                        Razlog = $"Greska na servisu placanja: {ex.Message}"
                    };
                    await bus.Publish("kotizacija-odbijena-events-ch", JsonSerializer.Serialize(odbijeno));
                    await bus.Publish("kotizacija-odbijena-prijave-ch", JsonSerializer.Serialize(odbijeno));
                }
            });

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
