namespace Placanja.API.Models.EventSourcing.Events
{
    public class SredstvaUplacena : DomainEvent
    {
        public double Iznos { get; set; }
    }
}
