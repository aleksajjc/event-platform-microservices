namespace Prijave.API.Models
{
    public class PrijavaZapocetaOutboxMessage
    {
        public int ID { get; set; }
        public Guid CorrelationId { get; set; }
        public OutboxMessageStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
