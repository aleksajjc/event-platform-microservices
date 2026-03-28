using EventPlatform.Domen;

namespace EventPlatform.Models.Lokacija
{
    public class LokacijaCreateViewModel
    {
        public int LokacijaID { get; set; }
        public string Naziv { get; set; }
        public string Adresa { get; set; }
        public int Kapacitet { get; set; }
    }
}
