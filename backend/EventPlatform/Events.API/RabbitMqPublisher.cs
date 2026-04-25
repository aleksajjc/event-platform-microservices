using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Threading.Channels;

namespace Events.API
{
    public interface IRabbitMqPublisher
    {
        Task PublishAsync(string payload, string messageId, string eventType, CancellationToken cancellationToken);
    }


    public sealed class RabbitMqPublisher : IRabbitMqPublisher, IAsyncDisposable
    {
        private readonly ConnectionFactory _factory;
        private readonly SemaphoreSlim _initLock = new(1, 1);
        private readonly RabbitMqOptions _options;

        private IConnection? connection;
        private IChannel? channel;

        public RabbitMqPublisher(IOptions<RabbitMqOptions> options)
        {
            _options = options.Value;

            _factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password
            };
        }

        public async Task PublishAsync(string payload, string messageId, string eventType, CancellationToken cancellationToken)
        {
            await EnsureInitializedAsync(cancellationToken);

            var body = Encoding.UTF8.GetBytes(payload);

            var properties = new BasicProperties
            {
                Persistent = true,
                MessageId = messageId,
                Type = eventType,
                ContentType = "application/json"
            };

            await channel.BasicPublishAsync(
                exchange: _options.Exchange,
                routingKey: _options.RoutingKey,
                mandatory: true,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken
            );
        }


        private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
        {
            if(channel is not null)
            {
                return;
            }

            await _initLock.WaitAsync(cancellationToken);
            try
            {
                if (channel is not null)
                {
                    return;
                }

                connection = await _factory.CreateConnectionAsync(cancellationToken);
                channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

                await channel.ExchangeDeclareAsync(
                    exchange: _options.Exchange,
                    type: ExchangeType.Direct,
                    durable: true,
                    autoDelete: false,
                    cancellationToken: cancellationToken);

                var glavniRedArgumenti = new Dictionary<string, object>
                {
                    { "x-dead-letter-exchange", "prijave.dlx" },
                    { "x-dead-letter-routing-key", "dlq.routing.key" }
                };

                await channel.QueueDeclareAsync(
                    queue: _options.Queue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: glavniRedArgumenti,
                    cancellationToken: cancellationToken); 

                await channel.QueueBindAsync(
                    queue: _options.Queue,
                    exchange: _options.Exchange,
                    routingKey: _options.RoutingKey,
                    cancellationToken: cancellationToken); 
            }
            finally
            {
                _initLock.Release();
            }
        }
        public async ValueTask DisposeAsync()
        {
            if(channel is not null)
            {
                await channel.DisposeAsync();
            }
            if(connection is not null)
            {
                await connection.DisposeAsync();
            }
            _initLock.Dispose();
        }
    }
}
