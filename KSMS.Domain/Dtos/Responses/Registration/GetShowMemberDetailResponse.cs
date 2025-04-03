using KSMS.Domain.Dtos.Responses.KoiMedium;
using KSMS.Domain.Dtos.Responses.RegistrationPayment;

namespace KSMS.Domain.Dtos.Responses.Registration;

public class GetShowMemberDetailResponse
{
    public Guid? ShowId { get; set; }
    public string? ShowName { get; set; }
    public string? ShowImageUrl { get; set; }
    public string? Location { get; set; }
    public string? Duration { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? CancellationReason { get; set; }
    public int TotalRegisteredKoi { get; set; }
    
    public List<RegistrationDetailItems> Registrations { get; set; } = [];

}

public class RegistrationDetailItems
{
    public Guid RegistrationId { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? Status { get; set; }
    public string? RefundType { get; set; }
    
    public string? RejectedReason { get; set; }
    public Guid KoiProfileId { get; set; }
    public string? KoiName { get; set; }
    public string? Variety { get; set; }
    public decimal? Size { get; set; }
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? BloodLine { get; set; }
    
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public decimal RegistrationFee { get; set; }
    
    public int? Rank { get; set; }
    public string? CurrentRound { get; set; }
    
    public string? EliminatedAtRound { get; set; }
    public List<AwardResponse> Awards { get; set; } = [];
    public RegistrationPaymentGetRegistrationResponse? Payment { get; set; }
    public List<GetKoiMediaResponse> Media { get; set; } = [];
}

public class AwardResponse
{
    public string? CategoryName { get; set; }
    public string? AwardType { get; set; }
    public string? AwardName { get; set; }
    public decimal? PrizeValue { get; set; }
}
