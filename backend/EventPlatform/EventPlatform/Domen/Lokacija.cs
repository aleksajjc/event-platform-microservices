namespace EventPlatform.Domen
{
    public class Lokacija
    {
        public int LokacijaID { get; set; }
        public string Naziv { get; set; }
        public string Adresa { get; set; }
        public int Kapacitet { get; set; }
        public List<StrucniDogadjaj> StrucniDogadjaji { get; set; }
    }
}
