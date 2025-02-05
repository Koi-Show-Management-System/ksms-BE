using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace KSMS.Domain.Dtos.Requests.KoiProfile;

public class CreateKoiProfileRequest
{
    [Required]
    public Guid VarietyId { get; set; }
    [Required]
    public required string Name { get; set; }
    [Required]
    public decimal Size { get; set; }
    [Required]
    public int Age { get; set; }
    [Required]
    public required string Gender { get; set; }
    [Required]
    public required string Bloodline { get; set; }
    [Required]
    public required string Status { get; set; }
    public IFormFile? Img { get; set; }
    public IFormFile? Video { get; set; }
}