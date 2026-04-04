using DTO.StrucniDogadjaji;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Predavaci
{
    public class PredavacDTO
    {
        public int PredavacID { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Titula { get; set; }
        public string OblastStrucnosti { get; set; }
    }
}
