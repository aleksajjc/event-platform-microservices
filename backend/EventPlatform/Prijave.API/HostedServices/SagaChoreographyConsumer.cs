using DTO.RabbitMq.Messages;
using Microsoft.EntityFrameworkCore;
using Prijave.API.Data;
using Prijave.API.Models;
using Prijave.API.Services;
using System.Text.Json;

namespace Prijave.API.HostedServices
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

            await bus.Subscribe<MestoOdbijeno>("mesto-odbijeno-ch", async (evt) =>
            {
                Console.WriteLine($"[CHOREOGRAPHY] [PRIJAVE] Received MestoOdbijeno. CorrelationID: {evt.CorrelationID}, Razlog: {evt.Razlog}");
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<PrijavaContext>();

                    var prijava = await db.Prijave
                        .FirstOrDefaultAsync(p => p.CorrelationID == evt.CorrelationID);

                    if (prijava != null)
                    {
                        prijava.StatusPrijava = StatusPrijava.Otkazana;
                        db.Prijave.Update(prijava);
                        await db.SaveChangesAsync();
                        Console.WriteLine($"[CHOREOGRAPHY] [PRIJAVE] Registration CANCELLED (No capacity). CorrelationID: {evt.CorrelationID}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CHOREOGRAPHY] [PRIJAVE ERROR] Error processing MestoOdbijeno: {ex.Message}");
                }
            });

            await bus.Subscribe<NaplacenaKotizacija>("kotizacija-naplacena-ch", async (evt) =>
            {
                Console.WriteLine($"[CHOREOGRAPHY] [PRIJAVE] Received NaplacenaKotizacija. CorrelationID: {evt.CorrelationID}");
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<PrijavaContext>();

                    var prijava = await db.Prijave
                        .FirstOrDefaultAsync(p => p.CorrelationID == evt.CorrelationID);

                    if (prijava != null)
                    {
                        prijava.StatusPrijava = StatusPrijava.Potvrdjena;
                        db.Prijave.Update(prijava);
                        await db.SaveChangesAsync();
                        Console.WriteLine($"[CHOREOGRAPHY] [PRIJAVE] Registration CONFIRMED successfully. CorrelationID: {evt.CorrelationID}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CHOREOGRAPHY] [PRIJAVE ERROR] Error processing NaplacenaKotizacija: {ex.Message}");
                }
            });

            await bus.Subscribe<OdbijenaKotizacija>("kotizacija-odbijena-prijave-ch", async (evt) =>
            {
                Console.WriteLine($"[CHOREOGRAPHY] [PRIJAVE] Received OdbijenaKotizacija. CorrelationID: {evt.CorrelationID}, Razlog: {evt.Razlog}");
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<PrijavaContext>();

                    var prijava = await db.Prijave
                        .FirstOrDefaultAsync(p => p.CorrelationID == evt.CorrelationID);

                    if (prijava != null)
                    {
                        prijava.StatusPrijava = StatusPrijava.Otkazana;
                        db.Prijave.Update(prijava);
                        await db.SaveChangesAsync();
                        Console.WriteLine($"[CHOREOGRAPHY] [PRIJAVE] Registration CANCELLED (Payment rejected). CorrelationID: {evt.CorrelationID}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CHOREOGRAPHY] [PRIJAVE ERROR] Error processing OdbijenaKotizacija: {ex.Message}");
                }
            });

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
