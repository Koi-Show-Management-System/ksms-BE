using KSMS.Domain.Dtos.Requests.RegistrationRound;
using KSMS.Domain.Dtos.Responses.RegistrationRound;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KSMS.Domain.Pagination;
using KSMS.Domain.Dtos.Requests.Registration;

namespace KSMS.Application.Services
{
    public interface IRegistrationRoundService
    {
        Task AssignMultipleFishesToTankAndRound(Guid roundId, List<Guid> registrationIds);
        Task UpdateFishesWithTanks(List<UpdateFishTankRequest> updateRequests);
        Task<RegistrationRoundResponse> CreateRegistrationRoundAsync(CreateRegistrationRoundRequest request);
        Task<Paginate<GetPageRegistrationRoundResponse>> GetPageRegistrationRound(Guid roundId, int page, int size);
        Task<CheckQrRegistrationRoundResponse> GetRegistrationRoundByIdAndRoundAsync(Guid registrationId, Guid roundId);
        Task PublishRound(Guid roundId);
        
    }
}
