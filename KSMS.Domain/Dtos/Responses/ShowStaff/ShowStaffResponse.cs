using KSMS.Domain.Dtos.Responses.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.ShowStaff
{
    public class ShowStaffResponse
    {
        public Guid Id { get; set; }

        public Guid ShowId { get; set; }

        public DateTime AssignedAt { get; set; }

        public virtual AccountResponse Account { get; set; } = null!;

        public virtual AccountResponse AssignedByNavigation { get; set; } = null!;
    }
}
