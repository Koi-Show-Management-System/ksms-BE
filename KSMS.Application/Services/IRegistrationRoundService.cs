using KSMS.Domain.Dtos.Requests.RegistrationRound;
using KSMS.Domain.Dtos.Responses.RegistrationRound;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KSMS.Domain.Pagination;

namespace KSMS.Application.Services
{
    public interface IRegistrationRoundService
    {

        Task<RegistrationRoundResponse> CreateRegistrationRoundAsync(CreateRegistrationRoundRequest request);
        Task<Paginate<GetPageRegistrationRoundResponse>> GetPageRegistrationRound(Guid roundId, int page, int size);
    }
}
