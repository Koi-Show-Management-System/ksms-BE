using KSMS.Domain.Dtos.Responses.Registration;
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
        public decimal PaidAmount { get; set; }

        public DateTime PaymentDate { get; set; }

        public string PaymentMethod { get; set; } = null!;

        public string Status { get; set; } = null!;

        public RegistrationCheckinResponse Registration { get; set; } = null!;

    }
}
