using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.RabbitMq.Messages
{
    public class RezervisiMesto
    {
        public Guid CorrelationID { get; set; }
        public int StrucniDogadjajID { get; set; }
    }
}
