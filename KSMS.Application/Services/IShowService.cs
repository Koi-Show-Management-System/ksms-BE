using KSMS.Domain.Dtos.Requests.Show;
using KSMS.Domain.Dtos.Responses.KoiShow;
using KSMS.Domain.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KSMS.Domain.Enums;

namespace KSMS.Application.Services
{
    public interface IShowService
    {
        Task<Paginate<PaginatedKoiShowResponse>> GetPagedShowsAsync(int page, int size);
        Task UpdateShowV2(Guid id, UpdateShowRequestV2 request);
        Task<GetKoiShowDetailResponse> GetShowDetailByIdAsync(Guid id);
        Task CreateShowAsync(CreateShowRequest request);
        Task<Paginate<GetMemberRegisterShowResponse>> GetMemberRegisterShowAsync(ShowStatus? showStatus, int page, int size);
        Task CancelShowAsync(Guid id, Domain.Enums.ShowStatus status, string? reason);
        Task<bool> UpdateShowStatusManually(Guid koiShowId, string statusName, bool setActive);
    }

}
