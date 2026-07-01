using Microsoft.EntityFrameworkCore;
using SagaOrkestrator.Data;
using SagaOrkestrator.Entities;
using SagaOrkestrator.Services;
using DTO.RabbitMq.Messages;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SagaOrkestrator
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            string sagaPattern = configuration["SagaPattern"] ?? "Orchestration";
            if (sagaPattern.Equals("Choreography", StringComparison.OrdinalIgnoreCase))
            {
                Console.Title = "- SAGA CHOREOGRAPHY -";
                Console.WriteLine("[SAGA] Orkestrator je DEAKTIVIRAN jer je aktivna Saga Koreografija.");
                while (true)
                {
                    await Task.Delay(1000);
                }
            }

            Console.Title = "- SAGA ORKESTRATOR CONSOLE -";
            Console.WriteLine("[SAGA] Pokretanje Saga Orkestratora...");

            
            using (var db = new SagaDbContext())
            {
                db.Database.EnsureCreated();
            }

            
            _ = Task.Run(() => Dispatcher.DispatchOutboxMessages());

            using var bus = new RabbitMqBus();

            
            await bus.Subscribe<PrijavaZapoceta>("prijava-zapoceta", async (evt) =>
            {
                Console.WriteLine($"[SAGA] Primljen događaj prijava-zapoceta. CorrelationId: {evt.CorrelationID}");
                try
                {
                    using var db = new SagaDbContext();

                    var vecPostoji = await db.SagaStates.AnyAsync(x => x.CorrelationID == evt.CorrelationID);
                    if (vecPostoji)
                    {
                        Console.WriteLine($"[SAGA] Saga već postoji za CorrelationId: {evt.CorrelationID}. Preskačem.");
                        return;
                    }

                    var sagaState = new SagaState
                    {
                        CorrelationID = evt.CorrelationID,
                        StrucniDogadjajID = evt.StrucniDogadjajID,
                        UcesnikID = evt.UcesnikID,
                        CenaKotizacije = evt.CenaKotizacije,
                        Status = SagaStatus.Started,
                        TrenutniKorak = "Započeto. Kreira se rezervacija mesta.",
                        CreatedAt = DateTime.UtcNow
                    };

                    var rezervisiMestoCmd = new RezervisiMesto
                    {
                        CorrelationID = evt.CorrelationID,
                        StrucniDogadjajID = evt.StrucniDogadjajID
                    };

                    var outbox = new SagaCommandOutboxMessage
                    {
                        CorrelationID = evt.CorrelationID,
                        QueueName = "rezervisi-mesto",
                        Payload = JsonSerializer.Serialize(rezervisiMestoCmd),
                        Status = OutboxMessageStatus.ForProcessing,
                        CreatedAt = DateTime.UtcNow
                    };

                    db.SagaStates.Add(sagaState);
                    db.SagaCommandOutboxMessages.Add(outbox);
                    await db.SaveChangesAsync();

                    Console.WriteLine($"[SAGA] Saga kreirana. Upisana outbox komanda za rezervaciju mesta za CorrelationId: {evt.CorrelationID}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SAGA ERROR] Greška u prijava-zapoceta handleru: {ex.Message}");
                }
            });

            // 2. MestoRezervisano -> Uspešno rezervisano mesto, šaljemo na naplatu
            await bus.Subscribe<MestoRezervisano>("mesto-rezervisano", async (evt) =>
            {
                Console.WriteLine($"[SAGA] Primljen događaj mesto-rezervisano. CorrelationId: {evt.CorrelationID}");
                try
                {
                    using var db = new SagaDbContext();

                    var sagaState = await db.SagaStates.FirstOrDefaultAsync(x => x.CorrelationID == evt.CorrelationID);
                    if (sagaState == null)
                    {
                        Console.WriteLine($"[SAGA ERROR] Saga nije pronađena za CorrelationId: {evt.CorrelationID}!");
                        return;
                    }

                    
                    var naplatiCmd = new NaplatiKotizaciju
                    {
                        CorrelationID = evt.CorrelationID,
                        UcesnikID = sagaState.UcesnikID,
                        Iznos = sagaState.CenaKotizacije
                    };

                    var outbox = new SagaCommandOutboxMessage
                    {
                        CorrelationID = evt.CorrelationID,
                        QueueName = "naplati-kotizaciju",
                        Payload = JsonSerializer.Serialize(naplatiCmd),
                        Status = OutboxMessageStatus.ForProcessing,
                        CreatedAt = DateTime.UtcNow
                    };

                    db.SagaCommandOutboxMessages.Add(outbox);
                    await db.SaveChangesAsync();

                    Console.WriteLine($"[SAGA] Mesto rezervisano. Upisana outbox komanda za naplatu za CorrelationId: {evt.CorrelationID}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SAGA ERROR] Greška u mesto-rezervisano handleru: {ex.Message}");
                }
            });

            
            await bus.Subscribe<MestoOdbijeno>("mesto-odbijeno", async (evt) =>
            {
                Console.WriteLine($"[SAGA] Primljen događaj mesto-odbijeno. Razlog: {evt.Razlog}. CorrelationId: {evt.CorrelationID}");
                try
                {
                    using var db = new SagaDbContext();

                    var sagaState = await db.SagaStates.FirstOrDefaultAsync(x => x.CorrelationID == evt.CorrelationID);
                    if (sagaState == null)
                    {
                        Console.WriteLine($"[SAGA ERROR] Saga nije pronađena za CorrelationId: {evt.CorrelationID}!");
                        return;
                    }

                    sagaState.Greska = evt.Razlog;
                    db.SagaStates.Update(sagaState);

                    
                    var otkaziCmd = new OtkaziPrijavu
                    {
                        CorrelationID = evt.CorrelationID
                    };

                    var outbox = new SagaCommandOutboxMessage
                    {
                        CorrelationID = evt.CorrelationID,
                        QueueName = "otkazi-prijavu",
                        Payload = JsonSerializer.Serialize(otkaziCmd),
                        Status = OutboxMessageStatus.ForProcessing,
                        CreatedAt = DateTime.UtcNow
                    };

                    db.SagaCommandOutboxMessages.Add(outbox);
                    await db.SaveChangesAsync();

                    Console.WriteLine($"[SAGA] Mesto odbijeno. Pokrenuta kompenzacija (otkazivanje) za CorrelationId: {evt.CorrelationID}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SAGA ERROR] Greška u mesto-odbijeno handleru: {ex.Message}");
                }
            });

            
            await bus.Subscribe<NaplacenaKotizacija>("kotizacija-naplacena", async (evt) =>
            {
                Console.WriteLine($"[SAGA] Primljen događaj kotizacija-naplacena. CorrelationId: {evt.CorrelationID}");
                try
                {
                    using var db = new SagaDbContext();

                    var sagaState = await db.SagaStates.FirstOrDefaultAsync(x => x.CorrelationID == evt.CorrelationID);
                    if (sagaState == null)
                    {
                        Console.WriteLine($"[SAGA ERROR] Saga nije pronađena za CorrelationId: {evt.CorrelationID}!");
                        return;
                    }

                    
                    var potvrdiCmd = new PotvrdiPrijavu
                    {
                        CorrelationID = evt.CorrelationID
                    };

                    var outbox = new SagaCommandOutboxMessage
                    {
                        CorrelationID = evt.CorrelationID,
                        QueueName = "potvrdi-prijavu",
                        Payload = JsonSerializer.Serialize(potvrdiCmd),
                        Status = OutboxMessageStatus.ForProcessing,
                        CreatedAt = DateTime.UtcNow
                    };

                    db.SagaCommandOutboxMessages.Add(outbox);
                    await db.SaveChangesAsync();

                    Console.WriteLine($"[SAGA] Kotizacija naplaćena. Upisana komanda za potvrdu prijave za CorrelationId: {evt.CorrelationID}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SAGA ERROR] Greška u kotizacija-naplacena handleru: {ex.Message}");
                }
            });

            // 5. KotizacijaOdbijena -> Odbijeno plaćanje, oslobađamo mesto i otkazujemo prijavu (kompenzacija)
            await bus.Subscribe<OdbijenaKotizacija>("kotizacija-odbijena", async (evt) =>
            {
                Console.WriteLine($"[SAGA] Primljen događaj kotizacija-odbijena. Razlog: {evt.Razlog}. CorrelationId: {evt.CorrelationID}");
                try
                {
                    using var db = new SagaDbContext();

                    var sagaState = await db.SagaStates.FirstOrDefaultAsync(x => x.CorrelationID == evt.CorrelationID);
                    if (sagaState == null)
                    {
                        Console.WriteLine($"[SAGA ERROR] Saga nije pronađena za CorrelationId: {evt.CorrelationID}!");
                        return;
                    }

                    sagaState.Greska = evt.Razlog;
                    db.SagaStates.Update(sagaState);

                    // Kompenzacija 1: Oslobodi mesto na Events.API
                    var oslobodiCmd = new OslobodiMesto
                    {
                        CorrelationID = evt.CorrelationID,
                        StrucniDogadjajID = sagaState.StrucniDogadjajID
                    };

                    var outboxOslobodi = new SagaCommandOutboxMessage
                    {
                        CorrelationID = evt.CorrelationID,
                        QueueName = "oslobodi-mesto",
                        Payload = JsonSerializer.Serialize(oslobodiCmd),
                        Status = OutboxMessageStatus.ForProcessing,
                        CreatedAt = DateTime.UtcNow
                    };

                    // Kompenzacija 2: Otkaži prijavu na Prijave.API
                    var otkaziCmd = new OtkaziPrijavu
                    {
                        CorrelationID = evt.CorrelationID
                    };

                    var outboxOtkazi = new SagaCommandOutboxMessage
                    {
                        CorrelationID = evt.CorrelationID,
                        QueueName = "otkazi-prijavu",
                        Payload = JsonSerializer.Serialize(otkaziCmd),
                        Status = OutboxMessageStatus.ForProcessing,
                        CreatedAt = DateTime.UtcNow
                    };

                    db.SagaCommandOutboxMessages.Add(outboxOslobodi);
                    db.SagaCommandOutboxMessages.Add(outboxOtkazi);
                    await db.SaveChangesAsync();

                    Console.WriteLine($"[SAGA] Kotizacija odbijena. Pokrenute kompenzacije (oslobodi-mesto i otkazi-prijavu) za CorrelationId: {evt.CorrelationID}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SAGA ERROR] Greška u kotizacija-odbijena handleru: {ex.Message}");
                }
            });

            Console.WriteLine("[SAGA] Saga Orkestrator je pokrenut i sluša poruke. Pritisnite CTRL+C za izlaz.");
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
