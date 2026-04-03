namespace EventPlatform.Domen
{
    public class StrucniDogadjaj
    {
        public int StrucniDogadjajID { get; set; }
        public string Naziv { get; set; }
        public string Agenda { get; set; }
        public DateTime DatumVremeOdrzavanja { get; set; }
        public double Trajanje { get; set; }
        public double CenaKotizacije { get; set; }
        public Lokacija Lokacija { get; set; }
        public int LokacijaID { get; set; }
        public List<Predavac> Predavaci { get; set; }
        public TipDogadjaja TipDogadjaja { get; set; }
        public int TipDogadjajaID { get; set; }
        public List<Prijava> Prijave { get; set; }
    }
}
