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
        Task<ScoreDetailResponse> CreateScoreAsync(CreateScoreDetailRequest request);

        Task<Paginate<ScoreDetailResponse>> GetPagedScoresAsync(int page, int size);
    }
}
