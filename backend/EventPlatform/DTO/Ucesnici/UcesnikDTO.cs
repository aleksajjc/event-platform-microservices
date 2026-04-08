using DTO.Prijave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Ucesnici
{
    public class UcesnikDTO
    {
        public int UcesnikID { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Email { get; set; }
        public List<PrijavaDTO> Prijave { get; set; }
    }
}
