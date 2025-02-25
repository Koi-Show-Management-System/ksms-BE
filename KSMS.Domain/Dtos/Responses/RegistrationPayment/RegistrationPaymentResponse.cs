using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.RegistrationPayment
{
    public class RegistrationPaymentResponse
    {
        public Guid Id { get; set; }

        public Guid RegistrationId { get; set; }

        public Guid? PaymentTypeId { get; set; }

        public string? QrcodeData { get; set; }

        public decimal PaidAmount { get; set; }

        public DateTime PaymentDate { get; set; }

        public string PaymentMethod { get; set; } = null!;

        public string Status { get; set; } = null!;

    }
}
