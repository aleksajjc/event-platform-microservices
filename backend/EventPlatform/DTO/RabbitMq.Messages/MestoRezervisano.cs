using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.RabbitMq.Messages
{
    public class MestoRezervisano
    {
        public Guid CorrelationID { get; set; }
        public int StrucniDogadjajID { get; set; }
        public int UcesnikID { get; set; }
        public double CenaKotizacije { get; set; }
    }
}
