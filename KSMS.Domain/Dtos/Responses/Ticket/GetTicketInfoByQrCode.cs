namespace KSMS.Domain.Dtos.Responses.Ticket;

public class GetTicketInfoByQrCode
{
    public Guid Id { get; set; }
    public string? Status { get; set; }
    public DateTime? CheckInTime { get; set; }
    public string? CheckInLocation { get; set; }
    public string? TicketTypeName { get; set; }
    public decimal? TicketPrice { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? ShowName { get; set; }
    public DateTime? ShowStartDate { get; set; }
    public DateTime? ShowEndDate { get; set; }
    public string? ShowLocation { get; set; }
    
}