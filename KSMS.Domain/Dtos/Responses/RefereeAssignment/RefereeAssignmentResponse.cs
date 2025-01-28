using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.RefereeAssignment
{
    public class RefereeAssignmentResponse
    {
        public Guid CategoryId { get; set; }

        public Guid RefereeAccountId { get; set; }

        public DateTime AssignedAt { get; set; }

        public Guid AssignedBy { get; set; }
    }
}
