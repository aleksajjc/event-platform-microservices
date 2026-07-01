namespace Placanja.API.Models.EventSourcing.Events
{
    public class RacunBlokiran : DomainEvent
    {
        public string Razlog { get; set; }
    }
}
