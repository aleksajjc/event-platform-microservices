using System;

namespace SagaOrkestrator.Entities
{
    public enum OutboxMessageStatus
    {
        ForProcessing,
        Processed
    }

    public class SagaCommandOutboxMessage
    {
        public int ID { get; set; }
        public Guid CorrelationID { get; set; }
        public string QueueName { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public OutboxMessageStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
