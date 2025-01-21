using System.ComponentModel.DataAnnotations;

namespace KSMS.Domain.Dtos.Requests.Authentication;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }
}