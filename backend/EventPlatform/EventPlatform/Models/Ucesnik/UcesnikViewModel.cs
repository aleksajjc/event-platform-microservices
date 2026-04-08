using EventPlatform.Models.Prijava;

namespace EventPlatform.Models.Ucesnik
{
    public class UcesnikViewModel
    {
        public int UcesnikID { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Email { get; set; }
        public List<PrijavaViewModel> Prijave { get; set; }
    }
}
