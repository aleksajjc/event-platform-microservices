using DTO.RabbitMq.Messages;
using Events.API.Data;
using Events.API.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Events.API.HostedServices
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

            await bus.Subscribe<PrijavaZapoceta>("prijava-zapoceta-ch", async (evt) =>
            {
                Console.WriteLine($"[CHOREOGRAPHY] [EVENTS] Received PrijavaZapoceta. CorrelationID: {evt.CorrelationID}, DogadjajID: {evt.StrucniDogadjajID}");
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<EventContext>();

                    var dogadjaj = await db.StrucniDogadjaji
                        .FirstOrDefaultAsync(d => d.StrucniDogadjajID == evt.StrucniDogadjajID);

                    if (dogadjaj == null)
                    {
                        Console.WriteLine($"[CHOREOGRAPHY] [EVENTS ERROR] Dogadjaj ID {evt.StrucniDogadjajID} not found!");
                        var odbijeno = new MestoOdbijeno
                        {
                            CorrelationID = evt.CorrelationID,
                            Razlog = "Dogadjaj nije pronadjen."
                        };
                        await bus.Publish("mesto-odbijeno-ch", JsonSerializer.Serialize(odbijeno));
                        return;
                    }

                    if (dogadjaj.SlobodnaMesta > 0)
                    {
                        dogadjaj.SlobodnaMesta--;
                        db.StrucniDogadjaji.Update(dogadjaj);
                        await db.SaveChangesAsync();

                        Console.WriteLine($"[CHOREOGRAPHY] [EVENTS] Seat reserved! CorrelationID: {evt.CorrelationID}, Remaining: {dogadjaj.SlobodnaMesta}");

                        var rezervisano = new MestoRezervisano
                        {
                            CorrelationID = evt.CorrelationID,
                            StrucniDogadjajID = evt.StrucniDogadjajID,
                            UcesnikID = evt.UcesnikID,
                            CenaKotizacije = evt.CenaKotizacije
                        };
                        await bus.Publish("mesto-rezervisano-ch", JsonSerializer.Serialize(rezervisano));
                    }
                    else
                    {
                        Console.WriteLine($"[CHOREOGRAPHY] [EVENTS] No capacity! CorrelationID: {evt.CorrelationID}, DogadjajID: {evt.StrucniDogadjajID}");
                        var odbijeno = new MestoOdbijeno
                        {
                            CorrelationID = evt.CorrelationID,
                            Razlog = "Nema slobodnih mesta."
                        };
                        await bus.Publish("mesto-odbijeno-ch", JsonSerializer.Serialize(odbijeno));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CHOREOGRAPHY] [EVENTS ERROR] Error processing reservation: {ex.Message}");
                    var odbijeno = new MestoOdbijeno
                    {
                        CorrelationID = evt.CorrelationID,
                        Razlog = $"Greska na Events servisu: {ex.Message}"
                    };
                    await bus.Publish("mesto-odbijeno-ch", JsonSerializer.Serialize(odbijeno));
                }
            });

            await bus.Subscribe<OdbijenaKotizacija>("kotizacija-odbijena-events-ch", async (evt) =>
            {
                Console.WriteLine($"[CHOREOGRAPHY] [EVENTS] Received OdbijenaKotizacija (Compensation). CorrelationID: {evt.CorrelationID}, DogadjajID: {evt.StrucniDogadjajID}");
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<EventContext>();

                    var dogadjaj = await db.StrucniDogadjaji
                        .FirstOrDefaultAsync(d => d.StrucniDogadjajID == evt.StrucniDogadjajID);

                    if (dogadjaj != null)
                    {
                        if (dogadjaj.SlobodnaMesta < dogadjaj.MaksimalanKapacitet)
                        {
                            dogadjaj.SlobodnaMesta++;
                            db.StrucniDogadjaji.Update(dogadjaj);
                            await db.SaveChangesAsync();
                            Console.WriteLine($"[CHOREOGRAPHY] [EVENTS] Seat released successfully! CorrelationID: {evt.CorrelationID}, New available count: {dogadjaj.SlobodnaMesta}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CHOREOGRAPHY] [EVENTS ERROR] Error processing compensation: {ex.Message}");
                }
            });

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
