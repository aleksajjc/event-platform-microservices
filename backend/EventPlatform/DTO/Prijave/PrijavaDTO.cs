using DTO.StrucniDogadjaji;
using DTO.Ucesnici;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Prijave
{
    public class PrijavaDTO
    {
        public  UcesnikDTO Ucesnik { get; set; }
        public int StrucniDogadjajID { get; set; }
        public DateTime DatumPrijave { get; set; }
    }
}
