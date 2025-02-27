
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.Ticket
{
    public class TicketTypeResponse
    {
        public Guid Id { get; set; }

        public Guid KoiShowId { get; set; }

        public string TicketType1 { get; set; } = null!;

        public decimal Price { get; set; }

        public int AvailableQuantity { get; set; }

    }
}
