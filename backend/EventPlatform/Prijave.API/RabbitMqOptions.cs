namespace Prijave.API
{
    public class RabbitMqOptions
    {
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string Exchange { get; set; } = "dogadjaji.exchange";
        public string Queue { get; set; } = "dogadjaji.kreiranje.queue";
        public string RoutingKey { get; set; } = "dogadjaji.kreiranje.routingkey";
        public ushort PrefetchCount { get; set; } = 1;
        public string RequestQueue { get; set; } = "dogadjaji.detalji.request";
        public string ReplyQueue { get; set; } = "dogadjaji.detalji.reply";
        public string EmailQueue { get; set; } = "email.slanje.queue";
    }
}
