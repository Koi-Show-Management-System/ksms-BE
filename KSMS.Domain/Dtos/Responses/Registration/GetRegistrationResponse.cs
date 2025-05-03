using KSMS.Domain.Dtos.Responses.Account;
using KSMS.Domain.Dtos.Responses.CompetitionCategory;
using KSMS.Domain.Dtos.Responses.KoiMedium;
using KSMS.Domain.Dtos.Responses.KoiProfile;
using KSMS.Domain.Dtos.Responses.KoiShow;

namespace KSMS.Domain.Dtos.Responses.Registration;

public class GetRegistrationResponse // for user
{
    public Guid Id { get; set; }

    public string? RegistrationNumber { get; set; }

    public string RegisterName { get; set; } = null!;

    public decimal KoiSize { get; set; }

    public int KoiAge { get; set; }

    public decimal RegistrationFee { get; set; }
    
    public string? QrcodeData { get; set; }

    public string? Status { get; set; }
    
    public string? RefundType { get; set; }
    
    public string? RejectedReason { get; set; }

    public string? Notes { get; set; }
    
    public DateTime? CheckInExpiredDate { get; set; }

    public bool? IsCheckedIn { get; set; }

    public DateTime? CheckInTime { get; set; }

    public string? CheckInLocation { get; set; }

    public Guid? CheckedInBy { get; set; }
    
    public DateTime? CreatedAt { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public GetCompetitionCategoryResponse? CompetitionCategory { get; set; }

    public ICollection<GetKoiMediaResponse> KoiMedia { get; set; } = new List<GetKoiMediaResponse>();

    public KoiProfileResponse? KoiProfile { get; set; }

    public GetKoiShowResponse? KoiShow { get; set; }
    public AccountResponse? Account { get; set; }
    public CheckOutKoiResponse? CheckOutLog { get; set; }
}