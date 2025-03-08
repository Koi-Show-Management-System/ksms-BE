using KSMS.Domain.Dtos.Responses.KoiMedium;
using KSMS.Domain.Dtos.Responses.Variety;

namespace KSMS.Domain.Dtos.Responses.KoiProfile;

public class GetAllKoiProfileResponse
{
    public Guid Id { get; set; }
    public string? Name { get; set; }

    public decimal? Size { get; set; }

    public int? Age { get; set; }

    public string? Gender { get; set; }

    public string? Bloodline { get; set; }

    public string? Status { get; set; }
    
    public DateTime? CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }

    public ICollection<GetKoiMediaResponse> KoiMedia { get; set; } = new List<GetKoiMediaResponse>();
    
    public VarietyResponse? Variety { get; set; }
}