using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace KSMS.Domain.Dtos.Requests.Account;

public class UpdateAccountRequest
{
    [Required]
    public required string FullName { get; set; } 
    [Required]
    public required string Username { get; set; }
    [Required]
    public required string Phone { get; set; }
    
    public IFormFile? AvatarUrl { get; set; }
    
}