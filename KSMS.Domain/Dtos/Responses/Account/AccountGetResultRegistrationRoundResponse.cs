using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.Account
{
    public class AccountGetResultRegistrationRoundResponse
    {
        public string? Email { get; set; }

        public string? FullName { get; set; }
        public string? Phone { get; set; }

        public string? Avatar { get; set; }
    }
}
