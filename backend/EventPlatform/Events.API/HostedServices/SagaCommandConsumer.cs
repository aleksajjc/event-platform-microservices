using DTO.RabbitMq.Messages;
using Events.API.Data;
using Events.API.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Events.API.HostedServices
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

            // Slušamo komandu za rezervaciju mesta
            await bus.Subscribe<RezervisiMesto>("rezervisi-mesto", async (cmd) =>
            {
                Console.WriteLine($"[EVENTS-SAGA] Primljena komanda RezervisiMesto za CorrelationId: {cmd.CorrelationID}, DogadjajID: {cmd.StrucniDogadjajID}");
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<EventContext>();

                    var dogadjaj = await db.StrucniDogadjaji
                        .FirstOrDefaultAsync(d => d.StrucniDogadjajID == cmd.StrucniDogadjajID);

                    if (dogadjaj == null)
                    {
                        Console.WriteLine($"[EVENTS-SAGA ERROR] Dogadjaj ID {cmd.StrucniDogadjajID} nije pronađen!");
                        var odbijeno = new MestoOdbijeno
                        {
                            CorrelationID = cmd.CorrelationID,
                            Razlog = "Događaj nije pronađen u bazi podataka."
                        };
                        await bus.Publish("mesto-odbijeno", JsonSerializer.Serialize(odbijeno));
                        return;
                    }

                    if (dogadjaj.SlobodnaMesta > 0)
                    {
                        dogadjaj.SlobodnaMesta--;
                        db.StrucniDogadjaji.Update(dogadjaj);
                        await db.SaveChangesAsync();

                        Console.WriteLine($"[EVENTS-SAGA] Mesto uspešno rezervisano! Preostalo slobodnih mesta: {dogadjaj.SlobodnaMesta}");

                        var rezervisano = new MestoRezervisano
                        {
                            CorrelationID = cmd.CorrelationID
                        };
                        await bus.Publish("mesto-rezervisano", JsonSerializer.Serialize(rezervisano));
                    }
                    else
                    {
                        Console.WriteLine($"[EVENTS-SAGA OUT_OF_CAPACITY] Nema slobodnih mesta za događaj ID: {cmd.StrucniDogadjajID}");
                        var odbijeno = new MestoOdbijeno
                        {
                            CorrelationID = cmd.CorrelationID,
                            Razlog = "Nema slobodnih mesta na ovom događaju."
                        };
                        await bus.Publish("mesto-odbijeno", JsonSerializer.Serialize(odbijeno));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[EVENTS-SAGA ERROR] Greška pri obradi RezervisiMesto: {ex.Message}");
                    var odbijeno = new MestoOdbijeno
                    {
                        CorrelationID = cmd.CorrelationID,
                        Razlog = $"Sistemska greška na Events.API: {ex.Message}"
                    };
                    await bus.Publish("mesto-odbijeno", JsonSerializer.Serialize(odbijeno));
                }
            });

            // Slušamo komandu za kompenzaciju (oslobađanje mesta)
            await bus.Subscribe<OslobodiMesto>("oslobodi-mesto", async (cmd) =>
            {
                Console.WriteLine($"[EVENTS-SAGA] Primljena kompenzaciona komanda OslobodiMesto za CorrelationId: {cmd.CorrelationID}, DogadjajID: {cmd.StrucniDogadjajID}");
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<EventContext>();

                    var dogadjaj = await db.StrucniDogadjaji
                        .FirstOrDefaultAsync(d => d.StrucniDogadjajID == cmd.StrucniDogadjajID);

                    if (dogadjaj != null)
                    {
                        // Oslobađamo mesto i pazimo da ne premašimo maksimalni kapacitet
                        if (dogadjaj.SlobodnaMesta < dogadjaj.MaksimalanKapacitet)
                        {
                            dogadjaj.SlobodnaMesta++;
                            db.StrucniDogadjaji.Update(dogadjaj);
                            await db.SaveChangesAsync();
                            Console.WriteLine($"[EVENTS-SAGA] Mesto uspešno OSLOBOĐENO! Novo stanje slobodnih mesta: {dogadjaj.SlobodnaMesta}");
                        }
                        else
                        {
                            Console.WriteLine($"[EVENTS-SAGA WARNING] Pokušaj oslobađanja mesta preko maksimalnog kapaciteta ({dogadjaj.MaksimalanKapacitet}) za događaj ID: {cmd.StrucniDogadjajID}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[EVENTS-SAGA ERROR] Dogadjaj ID {cmd.StrucniDogadjajID} nije pronađen za oslobađanje mesta!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[EVENTS-SAGA ERROR] Greška pri obradi OslobodiMesto: {ex.Message}");
                }
            });

            // Služi da pozadinska nit ostane aktivna
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
