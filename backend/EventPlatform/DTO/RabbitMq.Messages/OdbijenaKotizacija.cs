using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.RabbitMq.Messages
{
    public class OdbijenaKotizacija
    {
        public Guid CorrelationID { get; set; }
        public string Razlog { get; set; }
    }
}
