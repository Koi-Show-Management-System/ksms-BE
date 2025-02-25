using KSMS.Domain.Dtos.Requests.RegistrationRound;
using KSMS.Domain.Dtos.Responses.RegistrationRound;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Application.Services
{
    public interface IRegistrationRoundService
    {

        Task<RegistrationRoundResponse> CreateRegistrationRoundAsync(CreateRegistrationRoundRequest request);
        Task<RegistrationRoundResponse> GetRegistrationRoundAsync(Guid registrationId, Guid roundId);
    }
}
