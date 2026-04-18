
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Prijave.API.Data;
using Prijave.API.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace Prijave.API.HostedServices
{
    public class DogadjajCreatedConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private RabbitMqOptions _options;
        private ILogger<DogadjajCreatedConsumer> _logger;

        private IConnection connection;
        private IChannel channel;

        public DogadjajCreatedConsumer(IServiceScopeFactory scopeFactory, IOptions<RabbitMqOptions> options, ILogger<DogadjajCreatedConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password
            };

            connection = await factory.CreateConnectionAsync(stoppingToken);
            channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await channel.ExchangeDeclareAsync(
                exchange: _options.Exchange,
                type: ExchangeType.Direct, 
                durable: true, 
                autoDelete: false, 
                cancellationToken: stoppingToken);
            await channel.QueueDeclareAsync(
                queue: _options.Queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken);
            await channel.QueueBindAsync(
                queue: _options.Queue, 
                exchange: _options.Exchange, 
                routingKey: _options.RoutingKey, 
                cancellationToken: stoppingToken);

            await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: _options.PrefetchCount, global: false, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) => await HandleMessageAsync(ea, stoppingToken);

            await channel.BasicConsumeAsync(queue: _options.Queue,
                autoAck: false, 
                consumer: consumer, 
                cancellationToken: stoppingToken);
            _logger.LogInformation("Pokrenut consumer na queue: " + _options.Queue);

            try { 
                await Task.Delay(Timeout.Infinite, stoppingToken); 
            }
            catch (OperationCanceledException){ 
            }


        }

        private async Task HandleMessageAsync(BasicDeliverEventArgs ea, CancellationToken cancellationToken)
        {
            if (channel is null) return;
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PrijavaContext>();

                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                var dogadjajPutanja = JsonSerializer.Deserialize<DogadjajReferenceDTO>(body);

                await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

                var alreadyProcessed = await db.ProcessedMessages.AnyAsync(x => x.EventId == ea.BasicProperties.MessageId, cancellationToken);

                if (!alreadyProcessed)
                {
                    var newDogadjajRef = new DogadjajReference
                    {
                        StrucniDogadjajID = dogadjajPutanja.StrucniDogadjajID,
                        Naziv = dogadjajPutanja.Naziv
                    };
                    db.DogadjajReferences.Add(newDogadjajRef);
                    db.ProcessedMessages.Add(new ProcessedMessage
                    {
                        EventId = ea.BasicProperties.MessageId!,
                        EventType = "DogadjajKreiran",
                        ProcessedAtUtc = DateTime.UtcNow
                    });
                    await db.SaveChangesAsync(cancellationToken);
                    await tx.CommitAsync(cancellationToken);
                }
                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Krah pri obradi mreze!");
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: cancellationToken);
            }
        }

        public class DogadjajReferenceDTO { public int StrucniDogadjajID { get; set; } public string Naziv { get; set; } }
        public override void Dispose()
        {
            channel?.Dispose();
            connection?.Dispose();
            base.Dispose();
        }
    }
}
