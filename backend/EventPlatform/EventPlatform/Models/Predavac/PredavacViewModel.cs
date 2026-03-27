using EventPlatform.Domen;
using EventPlatform.Models.Dogadjaji;

namespace EventPlatform.Models.Predavac
{
    public class PredavacViewModel
    {
        public int PredavacID { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Titula { get; set; }
        public string OblastStrucnosti { get; set; }
        public List<DogadjajViewModel> OdabraniStrucniDogadjaji { get; set; }
    }
}
