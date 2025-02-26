using KSMS.Domain.Dtos.Requests.Variety;

namespace KSMS.Application.Services;

public interface IVarieryService
{
    Task CreateVariety(CreateVarietyRequest createVarietyRequest);
}