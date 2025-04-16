using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KSMS.Domain.Enums;
using Microsoft.AspNetCore.Http;


namespace KSMS.Domain.Dtos.Requests.Account
{
    public class CreateAccountRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required.")]
        [MinLength(5, ErrorMessage = "Username must be at least 5 characters long")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
        public string Username { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required.")]
        [EnumDataType(typeof(RoleName), ErrorMessage = "Invalid role specified.")]
        [StringLength(50, ErrorMessage = "Role cannot exceed 50 characters.")]
        public required string Role { get; set; }

        public IFormFile? AvatarUrl { get; set; }
    }
}


