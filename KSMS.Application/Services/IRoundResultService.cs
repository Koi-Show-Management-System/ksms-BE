using KSMS.Domain.Dtos.Requests.RoundResult;
using KSMS.Domain.Dtos.Responses.RoundResult;
using KSMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Application.Services
{
    public interface IRoundResultService
    {
        Task<GetAllRoundResultResponse> CreateRoundResultAsync(CreateRoundResult request);

        Task<GetAllRoundResultResponse> UpdateIsPublicAsync(Guid id, bool isPublic);
    }
}
