using KSMS.Domain.Dtos.Requests.RoundResult;
using KSMS.Domain.Dtos.Responses.Registration;
using KSMS.Domain.Dtos.Responses.RoundResult;
using KSMS.Domain.Entities;
using KSMS.Domain.Enums;
using KSMS.Domain.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Application.Services
{
    public interface IRoundResultService
    {
        // Task<RoundResultResponse> CreateRoundResultAsync(CreateRoundResult request);

        Task<Paginate<RegistrationGetByCategoryPagedResponse>> GetPagedRegistrationsByCategoryAndStatusAsync(Guid categoryId, RoundResultStatus? status, int page, int size);
        Task UpdateIsPublicByCategoryIdAsync(Guid categoryId, bool isPublic);
    }
}
