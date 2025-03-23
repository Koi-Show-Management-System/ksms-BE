namespace KSMS.Domain.Dtos.Responses.KoiShow;

public class GetMemberRegisterShowResponse
{
    public Guid Id { get; set; }
    public string? ShowName { get; set; }
    public string? ImageUrl { get; set; }
    public string? Location { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Status { get; set; }
    
    
}