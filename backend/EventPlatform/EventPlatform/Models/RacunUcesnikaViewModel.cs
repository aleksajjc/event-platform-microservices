namespace EventPlatform.Models
{
    public class RacunUcesnikaViewModel
    {
        public int Id { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Email { get; set; }
        public double StanjeNaRacunu { get; set; }
        public bool JeBlokiran { get; set; }
        public int Version { get; set; }
    }
}
