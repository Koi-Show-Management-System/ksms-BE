namespace KSMS.Domain.Dtos.Responses.Notification;

public class GetPageNotificationResponse
{
    public Guid? Id { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Type { get; set; }
    public bool? IsRead { get; set; }
    public DateTime? SentDate { get; set; }
    
}