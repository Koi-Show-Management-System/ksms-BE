using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KSMS.Domain.Dtos.Responses.Account;

namespace KSMS.Domain.Dtos.Responses.RefereeAssignment
{
    public class RefereeAssignmentResponse
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }

        public Guid RefereeAccountId { get; set; }

        public DateTime AssignedAt { get; set; }

         public Guid AssignedBy { get; set; }
        public AccountResponse RefereeAccount { get; set; } = null!;

        public AccountResponse AssignedByNavigation { get; set; } = null!;
    }
}
