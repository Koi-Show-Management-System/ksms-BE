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

    public string? Status { get; set; }

    public string? Notes { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public GetAllCompetitionCategoryResponse? CompetitionCategory { get; set; }

    public ICollection<GetKoiMediaResponse> KoiMedia { get; set; } = new List<GetKoiMediaResponse>();

    public KoiProfileResponse KoiProfile { get; set; } = null!;

    public GetKoiShowResponse KoiShow { get; set; } = null!;
 
    
    
    
    
}