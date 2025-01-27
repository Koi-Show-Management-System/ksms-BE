using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

 

    namespace KSMS.Domain.Dtos.Requests.Account
    {
        public class CreateAccountRequest
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [MinLength(3)]
            public string FullName { get; set; } = string.Empty;

            [Required]
            [Phone]
            public string Phone { get; set; } = string.Empty;

            [Required]
            [MinLength(5, ErrorMessage = "Username must be at least 5 characters long")]
            public string Username { get; set; } = string.Empty;

            [Required]
            [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
            public string Password { get; set; } = string.Empty;

            [Required]
            public Guid RoleId { get; set; }

            public string? Avatar { get; set; }
    }
    }

 
