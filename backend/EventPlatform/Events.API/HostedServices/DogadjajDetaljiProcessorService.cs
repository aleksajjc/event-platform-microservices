using Events.API.Data;
using Events.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace Events.API.HostedServices
{
    public class DogadjajDetaljiProcessorService : BackgroundService
    {
        private readonly RabbitMqOptions _options;
        private readonly IServiceScopeFactory _scopeFactory;
        public DogadjajDetaljiProcessorService(IOptions<RabbitMqOptions> options, IServiceScopeFactory scopeFactory)
        {
            _options = options.Value;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory { 
                HostName = _options.HostName 
            };
            await using var _connection = await factory.CreateConnectionAsync(stoppingToken);
            await using var _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await _channel.QueueDeclareAsync(queue: _options.RequestQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var request = JsonSerializer.Deserialize<DogadjajDetaljiRequest>(ea.Body.ToArray());
                    if (request is null) return;
                    Console.WriteLine("[Procesor Events.API] Klijent(Prijava.API) je poslao poruku za dogadjaj");
                    await Task.Delay(3000, stoppingToken);

                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<EventContext>();

                    var trazenDogadjaj = await dbContext.StrucniDogadjaji
                                        .Include(sd => sd.Lokacija)
                                        .FirstOrDefaultAsync(sd => sd.StrucniDogadjajID == request.DogadjajId);
                    if (trazenDogadjaj == null)
                    {
                        Console.WriteLine($"[Procesor Greška] Događaj sa ID {request.DogadjajId} NE POSTOJI u bazi!");
                        return; 
                    }

                    var response = new DogadjajDetaljiResponse(
                        trazenDogadjaj.StrucniDogadjajID,
                        trazenDogadjaj.Naziv,
                        trazenDogadjaj.Agenda ?? "Nema agende",
                        trazenDogadjaj.DatumVremeOdrzavanja,
                        new LokacijaInfo(trazenDogadjaj.Lokacija?.Naziv ?? "Nepoznato", trazenDogadjaj.Lokacija?.Adresa ?? "Nepoznato")
                    );

                    var responseBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));

                    var replyProps = new BasicProperties
                    {
                        CorrelationId = ea.BasicProperties.CorrelationId
                    };


                    var replyTo = ea.BasicProperties.ReplyTo;
                    if (!string.IsNullOrWhiteSpace(replyTo))
                    {
                        await _channel.BasicPublishAsync(exchange: string.Empty, routingKey: replyTo, mandatory: false, basicProperties: replyProps, body: responseBody);
                        Console.WriteLine($"[Procesor Events.API] Informacije izvučene iz baze i poslate nazad!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Procesor greška] {ex.Message}");
                }
                finally
                {
                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                }
            };
            await _channel.BasicConsumeAsync(queue: _options.RequestQueue, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException) { }
        }
    }
}
