using KSMS.Domain.Dtos.Responses.TicketOrderDetail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.TicketType
{
    public class TicketTypeRequest
    {
        public Guid Id { get; set; }

        //public Guid KoiShowId { get; set; }

        public string Name { get; set; } = null!;

        public decimal Price { get; set; }

        public int AvailableQuantity { get; set; }

    //    public virtual ICollection<TicketOrderDetailResponse> TicketOrderDetails { get; set; } = new List<TicketOrderDetailResponse>();

    }
}
