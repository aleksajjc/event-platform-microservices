using EventPlatform.Domen;
using EventPlatform.Models.Lokacija;
using EventPlatform.Models.Predavac;
using EventPlatform.Models.TipDogadjaja;

namespace EventPlatform.Models.Dogadjaji
{
    public class DogadjajViewModel
    {
        public int StrucniDogadjajID { get; set; }
        public string Naziv { get; set; }
        public string Agenda { get; set; }
        public DateTime DatumVremeOdrzavanja { get; set; }
        public double Trajanje { get; set; }
        public double CenaKotizacije { get; set; }
        public LokacijaViewModel Lokacija { get; set; }
        public List<PredavacViewModel> Predavaci { get; set; }
        public TipDogadjajaViewModel TipDogadjaja { get; set; }


    }
}
