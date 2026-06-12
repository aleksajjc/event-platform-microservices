namespace Events.API.CQRS.ReadModels
{

    public class DogadjajReadModel
    {
        public int StrucniDogadjajID { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public string Agenda { get; set; } = string.Empty;
        public DateTime DatumVremeOdrzavanja { get; set; }
        public double Trajanje { get; set; }
        public double CenaKotizacije { get; set; }
        public int MaksimalanKapacitet { get; set; }
        public int SlobodnaMesta { get; set; }
        public LokacijaReadModel Lokacija { get; set; } = new();
        public List<PredavacReadModel> Predavaci { get; set; } = new();
        public TipDogadjajaReadModel TipDogadjaja { get; set; } = new();
    }
}
