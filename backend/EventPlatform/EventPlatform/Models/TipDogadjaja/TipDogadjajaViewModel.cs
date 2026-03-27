using EventPlatform.Domen;
using EventPlatform.Models.Dogadjaji;

namespace EventPlatform.Models.TipDogadjaja
{
    public class TipDogadjajaViewModel
    {
        public int TipDogadjajaID { get; set; }
        public string NazivTipa { get; set; }
        public List<DogadjajViewModel> StrucniDogadjaji { get; set; }
    }
}
