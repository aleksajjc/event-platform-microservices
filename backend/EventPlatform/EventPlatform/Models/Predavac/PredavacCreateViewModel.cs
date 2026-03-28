using EventPlatform.Domen;

namespace EventPlatform.Models.Predavac
{
    public class PredavacCreateViewModel
    {
        public int PredavacID { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Titula { get; set; }
        public string OblastStrucnosti { get; set; }
        public List<int> StrucniDogadjajiIDs { get; set; }
    }
}
