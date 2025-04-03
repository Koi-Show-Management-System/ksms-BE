using KSMS.Domain.Dtos.Responses.KoiMedium;

namespace KSMS.Domain.Dtos.Responses.Vote;

public class GetFinalRegistrationResponse
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
    public GetRoundInfoResponse? RoundInfo { get; set; }
    public int VoteCount { get; set; }
    public AwardInfo? Award { get; set; }
    
    public class AwardInfo
    {
        public string? Name { get; set; }
        public string? AwardType { get; set; }
        public decimal? PrizeValue { get; set; }
    }
}