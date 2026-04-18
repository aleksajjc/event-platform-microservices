using System.ComponentModel.DataAnnotations;

namespace Prijave.API.Models
{
    public class ProcessedMessage
    {
        [Key]
        public string EventId { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public DateTime ProcessedAtUtc { get; set; }
    }
}
