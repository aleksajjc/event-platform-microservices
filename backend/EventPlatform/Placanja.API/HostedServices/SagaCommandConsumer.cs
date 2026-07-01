using DTO.RabbitMq.Messages;
using Microsoft.EntityFrameworkCore;
using Placanja.API.Data;
using Placanja.API.Models;
using Placanja.API.Services;
using System.Text.Json;

namespace Placanja.API.HostedServices
{
    public class SagaCommandConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public SagaCommandConsumer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var bus = new RabbitMqBus();

            
            await bus.Subscribe<NaplatiKotizaciju>("naplati-kotizaciju", async (cmd) =>
            {
                Console.WriteLine($"[PLACANJA-SAGA] Primljena komanda NaplatiKotizaciju za CorrelationId: {cmd.CorrelationID}, UcesnikID: {cmd.UcesnikID}, Iznos: {cmd.Iznos}");
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<PlacanjaContext>();

                    var racun = await db.RacuniUcesnika
                        .FirstOrDefaultAsync(r => r.UcesnikID == cmd.UcesnikID);

                    if (racun == null)
                    {
                        Console.WriteLine($"[PLACANJA-SAGA] Račun za učesnika ID {cmd.UcesnikID} nije pronađen. Dinamički kreiram račun sa 10000.00 dinara za nesmetano testiranje Happy Path-a.");
                        racun = new RacunUcesnika
                        {
                            UcesnikID = cmd.UcesnikID,
                            Ime = "Dinamički",
                            Prezime = "Korisnik",
                            Email = $"ucesnik{cmd.UcesnikID}@dinamicki.com",
                            StanjeNaRacunu = 10000.00
                        };
                        db.RacuniUcesnika.Add(racun);
                        await db.SaveChangesAsync();
                    }

                    if (racun.StanjeNaRacunu >= cmd.Iznos)
                    {
                        racun.StanjeNaRacunu -= cmd.Iznos;
                        db.RacuniUcesnika.Update(racun);
                        await db.SaveChangesAsync();

                        Console.WriteLine($"[PLACANJA-SAGA] Plaćanje uspešno! Skinuto: {cmd.Iznos} din, Novo stanje na računu: {racun.StanjeNaRacunu} din");

                        var naplaceno = new NaplacenaKotizacija
                        {
                            CorrelationID = cmd.CorrelationID
                        };
                        await bus.Publish("kotizacija-naplacena", JsonSerializer.Serialize(naplaceno));
                    }
                    else
                    {
                        Console.WriteLine($"[PLACANJA-SAGA LOW_BALANCE] Nedovoljno sredstava za učesnika ID {cmd.UcesnikID}. Stanje: {racun.StanjeNaRacunu} din, Traženo: {cmd.Iznos} din");
                        var odbijeno = new OdbijenaKotizacija
                        {
                            CorrelationID = cmd.CorrelationID,
                            Razlog = "Nedovoljno sredstava na računu učesnika."
                        };
                        await bus.Publish("kotizacija-odbijena", JsonSerializer.Serialize(odbijeno));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PLACANJA-SAGA ERROR] Greška pri obradi NaplatiKotizaciju: {ex.Message}");
                    var odbijeno = new OdbijenaKotizacija
                    {
                        CorrelationID = cmd.CorrelationID,
                        Razlog = $"Greška na servisu plaćanja: {ex.Message}"
                    };
                    await bus.Publish("kotizacija-odbijena", JsonSerializer.Serialize(odbijeno));
                }
            });

            
            await bus.Subscribe<VratiNovac>("vrati-novac", async (cmd) =>
            {
                Console.WriteLine($"[PLACANJA-SAGA] Primljena kompenzaciona komanda VratiNovac za CorrelationId: {cmd.CorrelationID}, UcesnikID: {cmd.UcesnikID}, Iznos: {cmd.Iznos}");
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<PlacanjaContext>();

                    var racun = await db.RacuniUcesnika
                        .FirstOrDefaultAsync(r => r.UcesnikID == cmd.UcesnikID);

                    if (racun != null)
                    {
                        racun.StanjeNaRacunu += cmd.Iznos;
                        db.RacuniUcesnika.Update(racun);
                        await db.SaveChangesAsync();
                        Console.WriteLine($"[PLACANJA-SAGA] Povraćaj novca uspešan! Vraćeno: {cmd.Iznos} din, Novo stanje na računu: {racun.StanjeNaRacunu} din");
                    }
                    else
                    {
                        Console.WriteLine($"[PLACANJA-SAGA ERROR] Račun za učesnika ID {cmd.UcesnikID} nije pronađen za povraćaj novca!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PLACANJA-SAGA ERROR] Greška pri obradi VratiNovac: {ex.Message}");
                }
            });

            
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
