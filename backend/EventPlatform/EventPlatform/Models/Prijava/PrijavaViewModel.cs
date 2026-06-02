using EventPlatform.Models.Dogadjaji;
using EventPlatform.Models.Predavac;
using EventPlatform.Models.Ucesnik;
using System;

namespace EventPlatform.Models.Prijava
{
    public class PrijavaViewModel
    {
        public UcesnikViewModel Ucesnik { get; set; }
        public DogadjajViewModel Dogadjaj { get; set; }
        public List<PredavacViewModel> Predavaci { get; set; }
        public string StatusPrijava { get; set; } = string.Empty;
        public Guid CorrelationID { get; set; }
    }
}
