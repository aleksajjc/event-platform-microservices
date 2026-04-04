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
    public class StrucniDogadjajDTO
    {
        public int StrucniDogadjajID { get; set; }
        public string Naziv { get; set; }
        public string Agenda { get; set; }
        public DateTime DatumVremeOdrzavanja { get; set; }
        public double Trajanje { get; set; }
        public double CenaKotizacije { get; set; }
        public LokacijaDTO Lokacija { get; set; }
        public List<PredavacDTO> Predavaci { get; set; }
        public TipDogadjajaDTO TipDogadjaja { get; set; }
    }
}
