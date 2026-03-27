using EventPlatform.Domen;
using EventPlatform.Models.Dogadjaji;

namespace EventPlatform.Models.Lokacija
{
    public class LokacijaViewModel
    {
        public int LokacijaID { get; set; }
        public string Naziv { get; set; }
        public string Adresa { get; set; }
        public int Kapacitet { get; set; }
        public List<DogadjajViewModel> OdabraniStrucniDogadjaji { get; set; }
    }
}
