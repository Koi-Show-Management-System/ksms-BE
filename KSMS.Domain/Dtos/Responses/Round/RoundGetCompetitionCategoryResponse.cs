namespace KSMS.Domain.Dtos.Responses.Round;

public class RoundGetCompetitionCategoryResponse
{
    public Guid Id { get; set; }
    public string? Name { get; set; }

    public int? RoundOrder { get; set; }

    public string RoundType { get; set; } = null!;

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public int? NumberOfRegistrationToAdvance { get; set; }

    public string? Status { get; set; }
}