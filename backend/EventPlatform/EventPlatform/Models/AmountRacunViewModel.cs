using System.ComponentModel.DataAnnotations;

namespace EventPlatform.Models
{
    public class AmountRacunViewModel
    {
        [Required]
        public int UcesnikID { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Iznos mora biti veci od 0.")]
        public double Iznos { get; set; }
    }
}
