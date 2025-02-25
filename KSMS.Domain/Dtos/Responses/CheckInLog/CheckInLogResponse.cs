using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.CheckInLog
{
    public class CheckInLogResponse
    {
        public Guid Id { get; set; }

        public Guid? TicketId { get; set; }

        public Guid? RegistrationPaymentId { get; set; }

        public DateTime? CheckInTime { get; set; }

        public string? CheckInLocation { get; set; }

        public Guid? CheckedInBy { get; set; }

        public string? Status { get; set; }

        public string? Notes { get; set; }
    }
}
