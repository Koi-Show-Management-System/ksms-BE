using System.ComponentModel.DataAnnotations;

namespace KSMS.Domain.Dtos.Requests.TicketOrder;

public class CreateTicketOrderRequest
{
    public List<OrderTicketRequest> ListOrder { get; set; } = [];
    [Required]
    public required string FullName { get; set; }
    [Required]
    public required string Email { get; set; }
}