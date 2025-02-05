using KSMS.Domain.Dtos.Requests.Score;
using KSMS.Domain.Dtos.Responses;
using KSMS.Domain.Dtos.Responses.Score;
using KSMS.Domain.Pagination;
using System;
using System.Threading.Tasks;

namespace KSMS.Application.Services
{
    public interface IScoreService
    {
        Task<ScoreResponse> CreateScoreAsync(CreateScoreRequest request);

        Task<Paginate<ScoreResponse>> GetPagedScoresAsync(int page, int size);
    }
}
