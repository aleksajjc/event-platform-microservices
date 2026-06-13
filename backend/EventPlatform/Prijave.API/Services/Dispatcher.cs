using DTO.RabbitMq.Messages;
using Microsoft.EntityFrameworkCore;
using Prijave.API.Data;
using Prijave.API.Models;
using System.Text.Json;

namespace Prijave.API.Services
{
    public class Dispatcher : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;

        public Dispatcher(IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
        }

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

                                string sagaPattern = _configuration["SagaPattern"] ?? "Orchestration";
                                string queueName = sagaPattern.Equals("Choreography", StringComparison.OrdinalIgnoreCase)
                                    ? "prijava-zapoceta-ch"
                                    : "prijava-zapoceta";

                                using var bus = new RabbitMqBus();
                                await bus.Publish(queueName, JsonSerializer.Serialize(prijavaZapoceta));

                                outboxMessage.Status = OutboxMessageStatus.Processed;

                                dbContext.Update(outboxMessage);
                                await dbContext.SaveChangesAsync(stoppingToken);

                                Console.WriteLine($"[PRIJAVE-DISPATCHER] Sent prijava-zapoceta (queue: {queueName}) for CorrelationId: {prijava.CorrelationID}");
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
