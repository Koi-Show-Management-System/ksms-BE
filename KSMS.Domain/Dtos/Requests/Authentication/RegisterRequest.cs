using System.ComponentModel.DataAnnotations;

namespace KSMS.Domain.Dtos.Requests.Authentication;

public class RegisterRequest
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
    public required string Email { get; set; }
    
    [Required(ErrorMessage = "Password is required.")]
    //[MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    [StringLength(255, ErrorMessage = "Password cannot exceed 255 characters.")]
    public required string Password { get; set; }
    
    [Required(ErrorMessage = "Username is required.")]
    [MinLength(5, ErrorMessage = "Username must be at least 5 characters long.")]
    [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
    public required string Username { get; set; }
    
    [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
    public required string FullName { get; set; }
}