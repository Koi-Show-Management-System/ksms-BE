using KSMS.Domain.Dtos.Responses.Ticket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.TicketOrderDetail
{
    public class TicketOrderDetailResponse
    {
        public Guid Id { get; set; }

        public Guid TicketOrderId { get; set; }

        public Guid TicketTypeId { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Amount { get; set; }

        public virtual ICollection<TicketResponse> Tickets { get; set; } = new List<TicketResponse>();
    }
}
