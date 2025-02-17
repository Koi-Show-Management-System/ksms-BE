using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace KSMS.Domain.Dtos.Requests.Registration;

public class CreateRegistrationRequest
{
    [Required]
    public Guid KoiShowId { get; set; }
    [Required]
    public Guid KoiProfileId { get; set; }
    [Required]
    public required string RegisterName { get; set; }
    
    public string? Notes { get; set; }
    
    public List<IFormFile> RegistrationImages { get; set; } = [];
    public List<IFormFile> RegistrationVideos { get; set; } = [];
}