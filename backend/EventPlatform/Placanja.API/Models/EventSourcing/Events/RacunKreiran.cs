namespace Placanja.API.Models.EventSourcing.Events
{
    public class RacunKreiran : DomainEvent
    {
        public int UcesnikID { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Email { get; set; }
    }
}
