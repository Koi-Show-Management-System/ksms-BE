using KSMS.Domain.Dtos.Responses.ShowStaff;
using KSMS.Domain.Enums;
using KSMS.Domain.Pagination;

namespace KSMS.Application.Services;

public interface IShowStaffService
{
    Task AddStaffOrManager(Guid showId, Guid accountId);
    
    Task RemoveStaffOrManager(Guid id);
    
    Task<Paginate<GetPageStaffAndManagerResponse>> GetPageStaffAndManager(Guid showId, RoleName? role, int page, int size);
}