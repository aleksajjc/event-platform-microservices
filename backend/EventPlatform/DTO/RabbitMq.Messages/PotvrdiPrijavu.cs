using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.RabbitMq.Messages
{
    public class PotvrdiPrijavu
    {
        public int CorrelationID { get; set; }
        public int StrucniDogadjajID { get; set; }
        public int UcesnikID { get; set; }
    }
}
