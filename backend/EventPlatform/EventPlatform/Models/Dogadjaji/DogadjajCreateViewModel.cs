namespace EventPlatform.Models.Dogadjaji
{
    public class DogadjajCreateViewModel
    {
        public int StrucniDogadjajID { get; set; }
        public string Naziv { get; set; }
        public string Agenda { get; set; }
        public DateTime DatumVremeOdrzavanja { get; set; }
        public double Trajanje { get; set; }
        public double CenaKotizacije { get; set; }
        public int LokacijaID { get; set; }
        public List<int> OdabraniPredavaciID { get; set; }
        public int TipDogadjajaID { get; set; }
    }
}
