namespace Placanja.API.Models.EventSourcing
{
    public abstract class AggregateSnapshot
    {
        public int ID { get; set; }
        public int Version { get; set; }
    }
}
