namespace Events.API.Models
{
    public class OutboxMessage
    {
        public long ID { get; set; }
        public string EventType { get; set; }
        public string Payload { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
