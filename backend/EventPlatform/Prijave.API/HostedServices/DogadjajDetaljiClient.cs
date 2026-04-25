
using Microsoft.Extensions.Options;
using Prijave.API.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace Prijave.API.HostedServices
{
    public class DogadjajDetaljiClient : IHostedService
    {
        private readonly RabbitMqOptions _options;

        private readonly ConcurrentDictionary<string, DogadjajDetaljiRequest> _pendingRequest = new();

        private IConnection _connection;
        private IChannel _publishChannel;
        private IChannel _consumerChannel;

        public DogadjajDetaljiClient(IOptions<RabbitMqOptions> options)
        {
            _options = options.Value; 
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName
            };
            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _publishChannel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
            _consumerChannel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await _publishChannel.QueueDeclareAsync(queue: _options.RequestQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);
            await _consumerChannel.QueueDeclareAsync(queue: _options.ReplyQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new AsyncEventingBasicConsumer(_consumerChannel);
            consumer.ReceivedAsync += ObradiOdgovorAsync;

            await _consumerChannel.BasicConsumeAsync(
                queue: _options.ReplyQueue,
                autoAck: false,
                consumer: consumer,
                cancellationToken: cancellationToken
                );
            Console.WriteLine($"[Klijent Prijave.API] Uspešno upaljen, slušam odgovore na redu: {_options.ReplyQueue}");
        }

        private async Task ObradiOdgovorAsync(object sender, BasicDeliverEventArgs ea)
        {
            if(_consumerChannel is null)
            {
                return;
            }
            try
            {
                var correlationId = ea.BasicProperties.CorrelationId;

                if(string.IsNullOrEmpty(correlationId) || !_pendingRequest.TryRemove(correlationId, out var originalniZahtev))
                {
                    return;
                }
                var response = JsonSerializer.Deserialize<DogadjajDetaljiResponse>(ea.Body.ToArray());
                if(response is null)
                {
                    return;
                }
                Console.WriteLine($"[Klijent] Stigao odgovor! {response.Naziv} {response.Agenda} {response.DatumOdrzavanja} {response.Lokacija.Naziv},{response.Lokacija.Adresa}");
            }
            finally
            {
                await _consumerChannel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        internal async Task<string> PosaljiZahtevAsync(DogadjajDetaljiRequest request)
        {
            if (_publishChannel is null)
            {
                throw new InvalidOperationException("Klijent nije upaljen!");
            }

            var correlationId = Guid.NewGuid().ToString();

            var requestBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request));

            _pendingRequest[correlationId] = request;

            var properties = new BasicProperties
            {
                CorrelationId = correlationId,
                ReplyTo = _options.ReplyQueue
            };
            await _publishChannel.BasicPublishAsync(exchange: string.Empty, routingKey: _options.RequestQueue, mandatory: false, basicProperties: properties, body: requestBody);
            Console.WriteLine($"[KLIJENT] Poslao sam zahtev! Pitam Events.API za dogadjaj");

            return correlationId;
        }
    }
}
