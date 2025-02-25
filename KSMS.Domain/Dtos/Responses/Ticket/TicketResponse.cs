using KSMS.Domain.Dtos.Responses.CheckInLog;
using KSMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.Ticket
{
    public class TicketResponse
    {
        public Guid Id { get; set; }

        public Guid TicketOrderDetailId { get; set; }

        public string? QrcodeData { get; set; }

        public DateTime ExpiredDate { get; set; }

        public bool? IsUsed { get; set; }

        public virtual CheckInLogResponse? CheckInLog { get; set; }
    }
}
