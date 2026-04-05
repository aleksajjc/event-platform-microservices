namespace Events.API.Models
{
    public class TipDogadjaja
    {
        public int TipDogadjajaID { get; set; }
        public string NazivTipa { get; set; }
        public List<StrucniDogadjaj> StrucniDogadjaji { get; set; }
    }
}
