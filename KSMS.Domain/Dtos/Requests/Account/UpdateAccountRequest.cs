using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace KSMS.Domain.Dtos.Requests.Account;

public class UpdateAccountRequest
{
    public string? FullName { get; set; } 

    public string? Username { get; set; }
    
    public string? Phone { get; set; }
    
    public IFormFile? AvatarUrl { get; set; }
    
}