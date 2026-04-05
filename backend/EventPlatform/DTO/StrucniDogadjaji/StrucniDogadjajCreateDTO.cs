using DTO.Lokacije;
using DTO.Predavaci;
using DTO.TipoviDogadjaja;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.StrucniDogadjaji
{
    public class StrucniDogadjajCreateDTO
    {
        public int StrucniDogadjajID { get; set; }
        public string Naziv { get; set; }
        public string Agenda { get; set; }
        public DateTime DatumVremeOdrzavanja { get; set; }
        public double Trajanje { get; set; }
        public double CenaKotizacije { get; set; }
        public int LokacijaID { get; set; }
        public List<int> PredavaciIDs { get; set; }
        public int TipDogadjajaID { get; set; }
    }
}
