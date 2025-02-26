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
      //  public Guid Id { get; set; }
    //    public Guid CompetitionCategoryId { get; set; }

        public Guid RefereeAccountId { get; set; }

        public DateTime AssignedAt { get; set; }

        public string RoundType { get; set; }

        public Guid AssignedBy { get; set; }


       // public virtual CreateAccountRequest? RefereeAccount { get; set; } 
        }
}
