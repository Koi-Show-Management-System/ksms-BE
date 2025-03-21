using KSMS.Domain.Dtos.Responses.Account;

namespace KSMS.Domain.Dtos.Responses.Score;

public class GetScoreDetailByRegistrationRoundResponse
{
    public Guid Id { get; set; }
    
    public decimal? InitialScore { get; set; }
    public decimal? TotalPointMinus { get; set; }
    public string? Status { get; set; }
    public string? Comments { get; set; }
    public DateTime? CreatedAt { get; set; }
    public AccountResponse? RefereeAccount { get; set; } = null!;
    public List<CriteriaWithErrorResponse> CriteriaWithErrors { get; set; } = [];
}

public class CriteriaWithErrorResponse
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? Order { get; set; }
    public decimal? Weight { get; set; }
    public List<ScoreDetailErrorWithTypeResponse> Errors { get; set; } = [];
}

public class ScoreDetailErrorWithTypeResponse
{
    public Guid Id { get; set; }
    public string? ErrorTypeName { get; set; }
    public string? Severity { get; set; }
    public decimal? PointMinus { get; set; }
}