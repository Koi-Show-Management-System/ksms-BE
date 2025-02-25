
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.Ticket
{
    public class CreateTicketRequest
    {


        public Guid Id { get; set; }

        public Guid TicketOrderDetailId { get; set; }

        public string? QrcodeData { get; set; }

        public DateTime ExpiredDate { get; set; }

        public bool? IsUsed { get; set; }

        //public virtual CheckInLog? CheckInLog { get; set; }
    }
}
