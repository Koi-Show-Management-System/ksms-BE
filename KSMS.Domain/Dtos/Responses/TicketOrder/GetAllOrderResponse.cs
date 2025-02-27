namespace KSMS.Domain.Dtos.Responses.TicketOrder;

public class GetAllOrderResponse
{
    public Guid Id { get; set; }
    
    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;
    
    public DateTime OrderDate { get; set; }

    public string TransactionCode { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public string? PaymentMethod { get; set; }

    public string? Status { get; set; }
}

