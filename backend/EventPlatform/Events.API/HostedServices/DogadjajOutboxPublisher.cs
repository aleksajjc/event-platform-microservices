
using Events.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Events.API.HostedServices
{
    public class DogadjajOutboxPublisher : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DogadjajOutboxPublisher> _logger;
        public DogadjajOutboxPublisher(IServiceScopeFactory scopeFactory, ILogger<DogadjajOutboxPublisher> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();

                var db = scope.ServiceProvider.GetRequiredService<EventContext>();
                var publisher = scope.ServiceProvider.GetRequiredService<IRabbitMqPublisher>();

                var pendingMessages = await db.OutboxMessages
                    .OrderBy(m => m.CreatedAt)
                    .Take(5)
                    .ToListAsync(stoppingToken);

                foreach(var message in pendingMessages)
                {
                    try
                    {
                        await publisher.PublishAsync(
                            payload: message.Payload,
                            messageId: message.ID.ToString(),
                            eventType: message.EventType,
                            cancellationToken: stoppingToken
                         );
                        db.OutboxMessages.Remove(message);

                        await db.SaveChangesAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unexpected outbox publishing error.");
                    }
                }
            }
        }
    }
}
