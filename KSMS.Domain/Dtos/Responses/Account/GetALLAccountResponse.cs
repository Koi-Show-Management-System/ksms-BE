using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KSMS.Domain.Dtos.Responses.Role;

namespace KSMS.Domain.Dtos.Responses.Account
{
    public class GetALLAccountResponse
    {
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Status { get; set; }
        public string? Role { get; set; }
        public string? Avatar { get; set; }

    }
}
