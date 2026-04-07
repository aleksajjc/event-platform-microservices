namespace Prijave.API.Models
{
    public class Prijava
    {
        public int UcesnikID { get; set; }
        public Ucesnik Ucesnik { get; set; }
        public int StrucniDogadjajID { get; set; }
        public DateTime DatumPrijave { get; set; }
    }
}
