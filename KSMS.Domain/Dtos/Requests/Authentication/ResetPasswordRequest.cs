
using System.ComponentModel.DataAnnotations;

namespace KSMS.Domain.Dtos.Requests.Authentication;
public class ResetPasswordRequest
{
    [Required]
    public required string Email { get; set; }
    [Required]
    public required string OTP { get; set; }
    [Required]
    public required string NewPassword { get; set; }
} 