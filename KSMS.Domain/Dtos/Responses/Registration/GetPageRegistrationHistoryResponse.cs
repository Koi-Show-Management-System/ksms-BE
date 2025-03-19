using KSMS.Domain.Dtos.Responses.CompetitionCategory;
using KSMS.Domain.Dtos.Responses.KoiProfile;
using KSMS.Domain.Dtos.Responses.KoiShow;
using KSMS.Domain.Dtos.Responses.RegistrationPayment;

namespace KSMS.Domain.Dtos.Responses.Registration;

public class GetPageRegistrationHistoryResponse
{
    public Guid Id { get; set; }
    public GetKoiShowResponse? KoiShow { get; set; }

    public KoiProfileCheckinResponse? KoiProfile { get; set; }

    public decimal KoiSize { get; set; }

    public int KoiAge { get; set; }

    public GetCompetitionCategoryResponse? CompetitionCategory { get; set; }

    public int? Rank { get; set; }

    public string? Status { get; set; }
    
    public DateTime? CheckInExpiredDate { get; set; }

    public bool? IsCheckedIn { get; set; }

    public DateTime? CheckInTime { get; set; }

    public string? CheckInLocation { get; set; }

    public Guid? CheckedInBy { get; set; }

    public DateTime? ApprovedAt { get; set; }
    
    public DateTime? CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    public RegistrationPaymentGetRegistrationResponse? RegistrationPayment { get; set; }
}