using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.RegistrationRound
{
    public class CreateRegistrationRoundRequest
    {
        public Guid RegistrationId { get; set; }

        public Guid RoundId { get; set; }

        public DateTime? CheckInTime { get; set; }

        public DateTime? CheckOutTime { get; set; }

        public Guid TankId { get; set; }

        public string? Status { get; set; }

        public string? Notes { get; set; }

      
    }
}
