namespace Events.API.CQRS.ReadModels
{
    // Čitamo samo ono što nam treba za prikaz.
    public class LokacijaReadModel
    {
        public int LokacijaID { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public string Adresa { get; set; } = string.Empty;
        public int Kapacitet { get; set; }
    }
}
