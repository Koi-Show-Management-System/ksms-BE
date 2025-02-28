using KSMS.Domain.Dtos.Responses.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.ShowStaff
{
    public class GetAllShowStaffResponse
    {
        public Guid Id { get; set; }

        public Guid KoiShowId { get; set; }

        public DateTime AssignedAt { get; set; }

        public  AccountResponse Account { get; set; } = null!;

        public  AccountResponse AssignedByNavigation { get; set; } = null!;
    }
}
