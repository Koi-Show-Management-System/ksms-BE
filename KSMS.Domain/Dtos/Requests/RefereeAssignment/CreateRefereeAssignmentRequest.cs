using KSMS.Domain.Dtos.Requests.Account;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.RefereeAssignment
{
    public class CreateRefereeAssignmentRequest
    {
        public Guid RefereeAccountId { get; set; }
        [Required]
        [MinLength(1, ErrorMessage = "RoundTypes must have at least one item.")]
        public List<string> RoundTypes { get; set; } = [];

    }
}
