namespace KSMS.Domain.Dtos.Requests.TicketOrder;

public class OrderTicketRequest
{
    public Guid TicketTypeId { get; set; }
    public int Quantity { get; set; }
}