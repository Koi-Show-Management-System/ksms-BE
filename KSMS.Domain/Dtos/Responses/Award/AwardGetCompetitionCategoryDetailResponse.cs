namespace KSMS.Domain.Dtos.Responses.Award;

public class AwardGetCompetitionCategoryDetailResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;

    public string? AwardType { get; set; }

    public decimal? PrizeValue { get; set; }

    public string? Description { get; set; }
}