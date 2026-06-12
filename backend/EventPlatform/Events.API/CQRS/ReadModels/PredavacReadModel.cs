namespace Events.API.CQRS.ReadModels
{
    public class PredavacReadModel
    {
        public int PredavacID { get; set; }
        public string Ime { get; set; } = string.Empty;
        public string Prezime { get; set; } = string.Empty;
        public string Titula { get; set; } = string.Empty;
        public string OblastStrucnosti { get; set; } = string.Empty;
    }
}
