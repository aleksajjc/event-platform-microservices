using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Events.API.Services
{
    public class RabbitMqBus : IDisposable
    {
        private IConnection _connection;
        private IChannel _channel;

        public RabbitMqBus()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
        }

        public async Task Subscribe<T>(string queueName, Func<T, Task> handler)
        {
            await _channel.QueueDeclareAsync(queueName, true, false, false);
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (sender, args) =>
            {
                try
                {
                    var body = args.Body.ToArray();
                    var payload = Encoding.UTF8.GetString(body);
                    var message = System.Text.Json.JsonSerializer.Deserialize<T>(payload);
                    if (message != null)
                        await handler(message);
                    await _channel.BasicAckAsync(args.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[EVENTS] Error: {ex.Message}");
                    await _channel.BasicNackAsync(args.DeliveryTag, false, true);
                }
            };
            await _channel.BasicConsumeAsync(queueName, false, consumer);
        }

        public async Task Publish(string queueName, string payload)
        {
            await _channel.QueueDeclareAsync(queueName, true, false, false);
            var body = Encoding.UTF8.GetBytes(payload);
            var properties = new BasicProperties() { Persistent = true };
            await _channel.BasicPublishAsync(string.Empty, queueName, true, properties, body);
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
