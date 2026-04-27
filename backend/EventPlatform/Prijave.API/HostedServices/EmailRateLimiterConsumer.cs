
using Microsoft.Extensions.Options;

namespace Prijave.API.HostedServices
{
    public class EmailRateLimiterConsumer : BackgroundService
    {
        private readonly RabbitMqOptions _options;
        public EmailRateLimiterConsumer(IOptions<RabbitMqOptions> options)
        {
            _options = options.Value;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
