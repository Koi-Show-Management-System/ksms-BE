namespace KSMS.Domain.Dtos.Requests.ShowStatus;

public class UpdateShowStatusRequestV2
{
    public string StatusName { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
}