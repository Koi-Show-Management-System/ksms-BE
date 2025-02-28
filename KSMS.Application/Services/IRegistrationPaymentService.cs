using KSMS.Domain.Dtos.Responses.RegistrationPayment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Application.Services
{
    public interface IRegistrationPaymentService
    {
        Task<RegistrationPaymentResponse> GetRegistrationPaymentByIdAsync(Guid id);
    }
}
