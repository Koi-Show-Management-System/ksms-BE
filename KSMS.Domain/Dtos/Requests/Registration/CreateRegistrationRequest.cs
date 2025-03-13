using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace KSMS.Domain.Dtos.Requests.Registration;

public class CreateRegistrationRequest
{
    [Required]
    public Guid KoiShowId { get; set; }
    [Required]
    public Guid CompetitionCategoryId { get; set; }
    [Required]
    public Guid KoiProfileId { get; set; }
    [Required]
    public required string RegisterName { get; set; }
    
    public string? Notes { get; set; }
    [MinLength(1, ErrorMessage = "Registration Images must have at least one item.")]
    public List<IFormFile> RegistrationImages { get; set; } = [];
    [MinLength(1, ErrorMessage = "Registration Videos must have at least one item.")]
    public List<IFormFile> RegistrationVideos { get; set; } = [];
}