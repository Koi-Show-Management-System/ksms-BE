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

        public Guid ShowId { get; set; }

        public string TicketType { get; set; } = null!;

        public decimal Price { get; set; }

        public int AvailableQuantity { get; set; }

    }
}
