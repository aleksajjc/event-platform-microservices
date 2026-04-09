using EventPlatform.Domen;

namespace EventPlatform.Models.Prijava
{
    public class PrijavaCreateViewModel
    {
        public int StrucniDogadjajID { get; set; }
        public int UcesnikID { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Email { get; set; }

    }
}
