using KSMS.Domain.Dtos.Responses.Variety;

namespace KSMS.Domain.Dtos.Responses.KoiProfile;

public class GetAllKoiProfileResponse
{
    public string? Name { get; set; }

    public decimal? Size { get; set; }

    public int? Age { get; set; }

    public string? Gender { get; set; }

    public string? Bloodline { get; set; }

    public string? Status { get; set; }

    public string? ImgUrl { get; set; }

    public string? VideoUrl { get; set; }
    
    public VarietyResponse? Variety { get; set; }
}