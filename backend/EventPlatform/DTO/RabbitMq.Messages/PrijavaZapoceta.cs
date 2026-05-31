using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Saga
{
    public class PrijavaZapoceta
    {
        public Guid CorrelationID { get; set; }
        public int StrucniDogadjajID { get; set; }
        public int UcenikID { get; set; }
        public double CenaKotizacije { get; set; }
        public string Email { get; set; }
    }
}
