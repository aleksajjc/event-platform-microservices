
using Microsoft.Extensions.Options;
using Prijave.API.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using System.Threading.Channels;

namespace Prijave.API.HostedServices
{
    public class EmailRateLimiterConsumer : BackgroundService
    {
        private readonly RabbitMqOptions _options;
        private readonly Queue<DateTime> poslednjih10Mejlova = new Queue<DateTime>();
        public EmailRateLimiterConsumer(IOptions<RabbitMqOptions> options)
        {
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName
            };
            var _connection = await factory.CreateConnectionAsync(stoppingToken);
            var _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await _channel.QueueDeclareAsync(
                    queue: _options.EmailQueue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );
            await _channel.BasicQosAsync(prefetchSize: 0,
                prefetchCount: 1,
                global: false, 
                cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    if (poslednjih10Mejlova.Count >= 10)
                    {
                        var najstarijiMejl = poslednjih10Mejlova.Peek();
                        var vremeOdSlanjaNajstarijeg = DateTime.UtcNow - najstarijiMejl;

                        if (vremeOdSlanjaNajstarijeg.TotalSeconds < 60)
                        {
                            var pauza = 60 - vremeOdSlanjaNajstarijeg.TotalSeconds;
                            Console.WriteLine($"[LIMITER] PAŽNJA: Dostignut limit od 10 mejlova u minuti! Pauziram obradu na {pauza} sekundi...");
                            await Task.Delay(TimeSpan.FromSeconds(pauza), stoppingToken);
                        }
                        poslednjih10Mejlova.Dequeue();
                    }
                    var mejl = JsonSerializer.Deserialize<EmailMessage>(ea.Body.ToArray());

                    var outboxPutanja = Path.Combine(Directory.GetCurrentDirectory(), "outbox");

                    if (!Directory.Exists(outboxPutanja))
                    {
                        Directory.CreateDirectory(outboxPutanja);
                    }
                    var imeFajla = $"email_{Guid.NewGuid()}.txt";
                    var sadrzaj = $"Email: {mejl.Email}\nNaslov: {mejl.Naslov}\nTekst: {mejl.TekstPoruke}";

                    await File.WriteAllTextAsync(Path.Combine(outboxPutanja, imeFajla), sadrzaj, stoppingToken);

                    poslednjih10Mejlova.Enqueue(DateTime.UtcNow);

                    Console.WriteLine($"[CONSUMER] 'Poslat' mejl za {mejl.Email}. (Sačuvan u {imeFajla})");
                }
                finally
                {
                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                }
            };
            await _channel.BasicConsumeAsync(queue: _options.EmailQueue, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

            while (!stoppingToken.IsCancellationRequested) await Task.Delay(1000, stoppingToken);
        }
    }
}
