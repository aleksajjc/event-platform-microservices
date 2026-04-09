using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Prijave
{
    public class PrijavaCreateDTO
    {
        public int UcesnikID { get; set; }
        public int StrucniDogadjajID { get; set; }
        public DateTime DatumPrijave { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Email { get; set; }
    }
}
