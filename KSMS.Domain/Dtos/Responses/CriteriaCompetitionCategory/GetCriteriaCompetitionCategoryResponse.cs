using KSMS.Domain.Dtos.Responses.Criterion;

namespace KSMS.Domain.Dtos.Responses.CriteriaCompetitionCategory;

public class GetCriteriaCompetitionCategoryResponse
{
    public Guid Id { get; set; }
    public string? RoundType { get; set; }

    public decimal? Weight { get; set; }

    public int? Order { get; set; }
    public CriteriaResponse? Criteria { get; set; }
}