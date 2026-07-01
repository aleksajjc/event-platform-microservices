using System;

namespace EventPlatform.Models
{
    public class EventHistoryViewModel
    {
        public string EventType { get; set; }
        public object EventData { get; set; }
        public DateTime OccurredOn { get; set; }
    }
}
