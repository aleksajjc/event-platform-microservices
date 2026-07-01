using System.ComponentModel.DataAnnotations;

namespace EventPlatform.Models
{
    public class CreateRacunViewModel
    {
        [Required]
        public int UcesnikID { get; set; }
        [Required]
        public string Ime { get; set; }
        [Required]
        public string Prezime { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
