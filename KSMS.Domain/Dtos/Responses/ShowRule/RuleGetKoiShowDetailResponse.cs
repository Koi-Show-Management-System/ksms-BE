namespace KSMS.Domain.Dtos.Responses.ShowRule;

public class RuleGetKoiShowDetailResponse
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;
}