using KSMS.Domain.Dtos.Responses.KoiMedium;

namespace KSMS.Domain.Dtos.Responses.Vote;

public class GetVotingResultResponse
{
    public Guid RegistrationId { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? RegisterName { get; set; }
    public string? CategoryName { get; set; }
    public string? KoiName { get; set; }
    public string? KoiVariety { get; set; }
    public decimal? Size { get; set; }
    public int Age { get; set; }
    public string? Gender { get; set; }
    public string? Bloodline { get; set; }
    public string? OwnerName { get; set; }
    public List<GetKoiMediaResponse> KoiMedia { get; set; } = [];
    public int VoteCount { get; set; }
    public int? Rank { get; set; }
    public GetFinalRegistrationResponse.AwardInfo? Award { get; set; }
}