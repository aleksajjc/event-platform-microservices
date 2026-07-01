namespace Placanja.API.Models.EventSourcing.Events
{
    public class SredstvaSkinuta : DomainEvent
    {
        public double Iznos { get; set; }
    }
}
