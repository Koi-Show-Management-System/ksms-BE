namespace KSMS.Domain.Dtos.Requests.Ticket;

public class UpdateTicketRequestV2
{
    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public int AvailableQuantity { get; set; }
}