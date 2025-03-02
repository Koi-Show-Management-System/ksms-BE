namespace KSMS.Domain.Dtos.Responses.TicketType;

public class TicketTypeGetKoiShowDetailResponse
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public decimal Price { get; set; }

    public int AvailableQuantity { get; set; }
}