using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace KSMS.Domain.Dtos.Requests.KoiProfile;

public class UpdateKoiProfileRequest
{
    public Guid? VarietyId { get; set; }
    public string? Name { get; set; }
    [Range(0, 999.99, ErrorMessage = "Size must be between 0 and 999.99.")]
    public decimal? Size { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Age must be greater than 0.")]
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? Bloodline { get; set; }
    public string? Status { get; set; }
    public List<IFormFile> KoiImages { get; set; } = [];
    public List<IFormFile> KoiVideos { get; set; } = [];
}