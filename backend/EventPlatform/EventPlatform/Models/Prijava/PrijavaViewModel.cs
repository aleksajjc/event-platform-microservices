using EventPlatform.Models.Dogadjaji;
using EventPlatform.Models.Predavac;
using EventPlatform.Models.Ucesnik;

namespace EventPlatform.Models.Prijava
{
    public class PrijavaViewModel
    {
        public UcesnikViewModel Ucesnik { get; set; }
        public DogadjajViewModel Dogadjaj { get; set; }
        public List<PredavacViewModel> Predavaci { get; set; }
    }
}
