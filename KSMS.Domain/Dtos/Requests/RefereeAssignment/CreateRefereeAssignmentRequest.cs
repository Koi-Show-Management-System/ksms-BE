using KSMS.Domain.Dtos.Requests.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.RefereeAssignment
{
    public class CreateRefereeAssignmentRequest
    {
        public Guid RefereeAccountId { get; set; }
        public List<string> RoundTypes { get; set; } = [];

    }
}
