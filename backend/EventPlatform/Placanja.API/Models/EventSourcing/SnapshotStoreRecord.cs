using System;

namespace Placanja.API.Models.EventSourcing
{
    public class SnapshotStoreRecord
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int AggregateId { get; set; }
        public string AggregateType { get; set; }
        public string SnapshotData { get; set; }
        public int Version { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
