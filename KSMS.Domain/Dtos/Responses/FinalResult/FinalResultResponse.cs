using KSMS.Domain.Dtos.Responses.KoiMedium;

namespace KSMS.Domain.Dtos.Responses.FinalResult;

public class FinalResultResponse
{
    public Guid RegistrationId { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? RegisterName { get; set; }
    public decimal KoiSize { get; set; }
    public int Rank { get; set; }
    public decimal FinalScore { get; set; }
    public string? Status { get; set; }
    public string? AwardType { get; set; }
    public string? AwardName { get; set; }
    public decimal? PrizeValue { get; set; }
    
    public string? KoiName { get; set; }
    public string? Bloodline { get; set; }
    public string? Gender { get; set; }
    public string? Variety { get; set; }

    public List<GetKoiMediaResponse> Media { get; set; } = [];
}