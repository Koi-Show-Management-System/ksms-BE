using Microsoft.AspNetCore.Http;

namespace KSMS.Domain.Dtos.Requests.KoiProfile;

public class UpdateKoiProfileRequest
{
    public Guid? VarietyId { get; set; }
    public string? Name { get; set; }
    public decimal? Size { get; set; }
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? Bloodline { get; set; }
    public string? Status { get; set; }
    public IFormFile? Img { get; set; }
    public IFormFile? Video { get; set; }
}