using KSMS.Domain.Dtos.Responses.TicketType;

namespace KSMS.Domain.Dtos.Responses.TicketOrder;

public class GetOrderDetailResponse
{
    public Guid Id { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public TicketTypeGetOrderDetailResponse TicketType { get; set; } = null!;
}