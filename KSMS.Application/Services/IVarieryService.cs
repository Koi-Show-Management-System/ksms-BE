using KSMS.Domain.Dtos.Requests.Variety;
using KSMS.Domain.Dtos.Responses.Variety;

namespace KSMS.Application.Services;

public interface IVarieryService
{
    Task CreateVariety(CreateVarietyRequest createVarietyRequest);

    Task<IEnumerable<VarietyResponse>> GetAllVarietyAsync();
}