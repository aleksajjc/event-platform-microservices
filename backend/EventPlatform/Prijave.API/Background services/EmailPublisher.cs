using Microsoft.Extensions.Options;
using Prijave.API.Models;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace Prijave.API.Background_services
{
    public class EmailPublisher
    {
        private readonly RabbitMqOptions _options;
        public EmailPublisher(IOptions<RabbitMqOptions> options)
        {
            _options = options.Value;
        }
        public async Task PosaljiEmailNaQueue(EmailMessage email)
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName
            };
            await using var _connection = await factory.CreateConnectionAsync();
            await using var _channel = await _connection.CreateChannelAsync();

            await _channel.QueueDeclareAsync(
                queue: _options.EmailQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
                );

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(email));

            await _channel.BasicPublishAsync(exchange: string.Empty, 
                routingKey: _options.EmailQueue, 
                mandatory: false, 
                basicProperties: new BasicProperties(), 
                body: body);

            Console.WriteLine($"[PUBLISHER] {email.Email} je predat u red za čekanje!");
        }
    }
}
