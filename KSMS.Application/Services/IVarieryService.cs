using KSMS.Domain.Dtos.Requests.Variety;
using KSMS.Domain.Dtos.Responses.Variety;
using KSMS.Domain.Pagination;

namespace KSMS.Application.Services;

public interface IVarieryService
{
    Task CreateVariety(CreateVarietyRequest createVarietyRequest);

    Task<Paginate<VarietyResponse>> GetPagingVariety(int page, int size);
}