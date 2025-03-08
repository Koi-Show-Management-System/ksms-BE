using KSMS.Domain.Dtos.Responses.CategoryVariety;

namespace KSMS.Domain.Dtos.Responses.CompetitionCategory;

public class GetCompetitionCategoryResponse
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = null!;
}