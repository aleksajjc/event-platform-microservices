using DTO.RabbitMq.Messages;
using Microsoft.EntityFrameworkCore;
using Prijave.API.Data;
using Prijave.API.Models;
using Prijave.API.Services;
using System.Text.Json;

namespace Prijave.API.HostedServices
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

            await bus.Subscribe<PotvrdiPrijavu>("potvrdi-prijavu", async (cmd) =>
            {
                Console.WriteLine($"[PRIJAVE-SAGA] Primljena komanda PotvrdiPrijavu za CorrelationID: {cmd.CorrelationID}");
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<PrijavaContext>();

                    var prijava = await db.Prijave
                        .FirstOrDefaultAsync(p => p.CorrelationID == cmd.CorrelationID);

                    if (prijava != null)
                    {
                        prijava.StatusPrijava = StatusPrijava.Potvrdjena;
                        db.Prijave.Update(prijava);
                        await db.SaveChangesAsync();
                        Console.WriteLine($"[PRIJAVE-SAGA] Prijava uspešno POTVRĐENA za CorrelationID: {cmd.CorrelationID}");
                    }
                    else
                    {
                        Console.WriteLine($"[PRIJAVE-SAGA ERROR] Prijava nije pronađena za CorrelationID: {cmd.CorrelationID}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PRIJAVE-SAGA ERROR] Greška pri obradi PotvrdiPrijavu: {ex.Message}");
                }
            });

            await bus.Subscribe<OtkaziPrijavu>("otkazi-prijavu", async (cmd) =>
            {
                Console.WriteLine($"[PRIJAVE-SAGA] Primljena komanda OtkaziPrijavu za CorrelationID: {cmd.CorrelationID}");
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<PrijavaContext>();

                    var prijava = await db.Prijave
                        .FirstOrDefaultAsync(p => p.CorrelationID == cmd.CorrelationID);

                    if (prijava != null)
                    {
                        prijava.StatusPrijava = StatusPrijava.Otkazana;
                        db.Prijave.Update(prijava);
                        await db.SaveChangesAsync();
                        Console.WriteLine($"[PRIJAVE-SAGA] Prijava uspešno OTKAZANA za CorrelationID: {cmd.CorrelationID}");
                    }
                    else
                    {
                        Console.WriteLine($"[PRIJAVE-SAGA ERROR] Prijava nije pronađena za CorrelationID: {cmd.CorrelationID}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PRIJAVE-SAGA ERROR] Greška pri obradi OtkaziPrijavu: {ex.Message}");
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
