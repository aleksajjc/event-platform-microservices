using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Placanja.API.Models
{
    public class RacunUcesnika
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int UcesnikID { get; set; }
        public string Ime { get; set; } = string.Empty;
        public string Prezime { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public double StanjeNaRacunu { get; set; }
    }
}
