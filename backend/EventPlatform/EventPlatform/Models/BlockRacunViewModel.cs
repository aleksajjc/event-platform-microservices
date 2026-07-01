using System.ComponentModel.DataAnnotations;

namespace EventPlatform.Models
{
    public class BlockRacunViewModel
    {
        [Required]
        public int UcesnikID { get; set; }
        
        [Required]
        public string Razlog { get; set; }
    }
}
