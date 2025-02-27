namespace KSMS.Domain.Dtos.Responses.Criterion;

public class GetAllCriteriaResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;

    public string? Description { get; set; }
}