namespace Events.API.Models
{
    public class Predavac
    {
        public int PredavacID { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Titula { get; set; }
        public string OblastStrucnosti { get; set; }
        public List<StrucniDogadjaj> StrucniDogadjaji { get; set; }
    }
}
