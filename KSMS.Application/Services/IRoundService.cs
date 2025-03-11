using KSMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KSMS.Domain.Dtos.Responses.Round;
using KSMS.Domain.Pagination;

namespace KSMS.Application.Services
{
    public interface IRoundService
    {
        Task UpdateRoundStatusAsync(Guid roundId);

        Task<Paginate<GetPageRoundResponse>> GetPageRound(Guid competitionCategoryId, int page, int size);

    }

}
