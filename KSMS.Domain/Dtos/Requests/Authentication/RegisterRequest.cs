using System.ComponentModel.DataAnnotations;

namespace KSMS.Domain.Dtos.Requests.Authentication;

public class RegisterRequest
{
    [Required]
    public required string Email { get; set; }
    [Required]
    public required string Password { get; set; } 
    [Required]
    public required string Username { get; set; }
    [Required]
    public required string FullName { get; set; }
}