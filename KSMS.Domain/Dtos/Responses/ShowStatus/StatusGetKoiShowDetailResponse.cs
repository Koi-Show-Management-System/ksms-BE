namespace KSMS.Domain.Dtos.Responses.ShowStatus;

public class StatusGetKoiShowDetailResponse
{
    public Guid Id { get; set; }

    public string? StatusName { get; set; }

    public string? Description { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; }
}