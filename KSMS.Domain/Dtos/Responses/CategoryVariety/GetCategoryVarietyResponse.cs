using KSMS.Domain.Dtos.Responses.Variety;

namespace KSMS.Domain.Dtos.Responses.CategoryVariety;

public class GetCategoryVarietyResponse
{
    public Guid Id { get; set; }
    public VarietyResponse? Variety { get; set; }
}