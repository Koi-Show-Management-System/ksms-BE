using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace KSMS.Domain.Dtos.Requests.Registration;

public class CreateRegistrationRequest
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
    public required IFormFile Img { get; set; }
    [Required]
    public required IFormFile Video { get; set; }
    [Required]
    public Guid CategoryId { get; set; }
    
    public string? Notes { get; set; }
    [Required]
    public required string PaymentMethod { get; set; }
}