namespace Placanja.API.Models.EventSourcing
{
    public class RacunUcesnikaSnapshot : AggregateSnapshot
    {
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Email { get; set; }
        public double StanjeNaRacunu { get; set; }
        public bool JeBlokiran { get; set; }
    }
}
