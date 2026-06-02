
using DTO.RabbitMq.Messages;
using Microsoft.EntityFrameworkCore;
using Prijave.API.Data;
using Prijave.API.Models;
using System.Text.Json;

namespace Prijave.API.Services
{
    public class Dispatcher(IServiceScopeFactory scopeFactory) : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<PrijavaContext>();

                        var outboxMessage = await dbContext.PrijavaZapocetaOutboxMessages
                            .FirstOrDefaultAsync(x => x.Status == OutboxMessageStatus.ForProcessing, stoppingToken);
                        if (outboxMessage != null)
                        {
                            var prijava = await dbContext.Prijave
                                .Include(p => p.Ucesnik)
                                .FirstOrDefaultAsync(x => x.CorrelationID == outboxMessage.CorrelationId, stoppingToken);
                            if (prijava != null)
                            {
                                var prijavaZapoceta = new PrijavaZapoceta
                                {
                                    CorrelationID = prijava.CorrelationID,
                                    StrucniDogadjajID = prijava.StrucniDogadjajID,
                                    UcesnikID = prijava.UcesnikID,
                                    Email = prijava.Ucesnik.Email,
                                    CenaKotizacije = prijava.CenaKotizacije 
                                };
                                using var bus = new RabbitMqBus();
                                await bus.Publish("prijava-zapoceta", JsonSerializer.Serialize(prijavaZapoceta));

                                outboxMessage.Status = OutboxMessageStatus.Processed;

                                dbContext.Update(outboxMessage);
                                await dbContext.SaveChangesAsync(stoppingToken);

                                Console.WriteLine($"[PRIJAVE-DISPATCHER] Poslata poruka prijava-zapoceta za CorrelationId: {prijava.CorrelationID}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PRIJAVE-DISPATCHER ERROR] {ex.Message}");
                }
                await Task.Delay(3000, stoppingToken);
            }
        }
    }
}
