namespace EventPlatform.Domen
{
    public class Ucesnik
    {
        public int UcesnikID { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Email { get; set; }
        public List<Prijava> Prijave { get; set; }
    }
}
