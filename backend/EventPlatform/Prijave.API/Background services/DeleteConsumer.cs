
using Microsoft.AspNetCore.Identity;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

using Prijave.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Prijave.API.Background_services
{
    public class DeleteConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DeleteConsumer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            const string host = "localhost";
            const string username = "guest";
            const string password = "guest";

            const string exchangeName = "prijave.exchange";
            const string queueName = "prijave.brisanje.queue";
            const string routingKey = "prijave.brisanje.routingkey";

            var factory = new ConnectionFactory
            {
                HostName = host,
                UserName = username,
                Password = password
            };

            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Direct, durable: true, autoDelete: false);
            await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false);
            await channel.QueueBindAsync(queueName, exchangeName, routingKey);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (sender, eventArgs) =>
            {
                try
                {
                    var body = eventArgs.Body.ToArray();
                    var messageJSON = Encoding.UTF8.GetString(body);
                    Console.WriteLine($"[RABBITMQ DEBUG] Primljena JSON poruka: {messageJSON}");

                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var poruka = JsonSerializer.Deserialize<PorukaBrisanje>(messageJSON, options);
                    
                    if(poruka != null)
                    {
                        Console.WriteLine($"[RABBITMQ DEBUG] Deserijalizovano: Ucesnik: {poruka.UcesnikID}, Dogadjaj: {poruka.DogadjajID}. Trazim u bazi...");

                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<PrijavaContext>();
                            
                            var prijavaZaBrisanje = await dbContext.Prijave
                                .FirstOrDefaultAsync(p => p.UcesnikID == poruka.UcesnikID && p.StrucniDogadjajID == poruka.DogadjajID);

                            if (prijavaZaBrisanje != null)
                            {
                                dbContext.Prijave.Remove(prijavaZaBrisanje);
                                await dbContext.SaveChangesAsync();
                                Console.WriteLine($"[RABBITMQ DEBUG] [USPESNO] Obrisana prijava iz baze (UcesnikID: {poruka.UcesnikID}, DogadjajID: {poruka.DogadjajID})!");
                            }
                            else
                            {
                                Console.WriteLine($"[RABBITMQ DEBUG] [UPOZORENJE] Prijava se ne nalazi u bazi!");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[RABBITMQ DEBUG] Greska pri deserijalizaciji: poruka je NULL.");
                    }
                    await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RABBITMQ EROOR] Izuzetak tokom brisanja: {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                    await channel.BasicRejectAsync(eventArgs.DeliveryTag, requeue: false);
                }
            };
            await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        private class PorukaBrisanje
        {
            public int UcesnikID { get; set; }
            public int DogadjajID { get; set; }
        }
    }
}
