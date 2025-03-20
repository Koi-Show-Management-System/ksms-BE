using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.ShowStatus;
using KSMS.Domain.Entities;
using KSMS.Domain.Exceptions;
using KSMS.Infrastructure.Database;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace KSMS.Infrastructure.Services;

public class ShowStatusService : BaseService<ShowStatusService>, IShowStatusService
{
    public ShowStatusService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<ShowStatusService> logger, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, httpContextAccessor)
    {
    }

    public async Task CreateShowStatusAsync(Guid koiShowId, CreateShowStatusRequest request)
    {
        var show = await _unitOfWork.GetRepository<KoiShow>().SingleOrDefaultAsync(predicate: x => x.Id == koiShowId);
        if (show == null)
        {
            throw new NotFoundException("Không tìm thấy cuộc thi");
        }
        var status = request.Adapt<ShowStatus>();
        status.KoiShowId = koiShowId;
        await _unitOfWork.GetRepository<ShowStatus>().InsertAsync(status);
        await _unitOfWork.CommitAsync();
    }

    public async Task UpdateShowStatusAsync(Guid id, UpdateShowStatusRequestV2 request)
    {
        var status = await _unitOfWork.GetRepository<ShowStatus>().SingleOrDefaultAsync(predicate: x => x.Id == id);
        if (status == null)
        {
            throw new NotFoundException("Không tìm thấy trạng thái cuộc thi");
        }
        request.Adapt(status);
        _unitOfWork.GetRepository<ShowStatus>().UpdateAsync(status);
        await _unitOfWork.CommitAsync();
    }

    public async Task DeleteShowStatusAsync(Guid id)
    {
        var status = await _unitOfWork.GetRepository<ShowStatus>().SingleOrDefaultAsync(predicate: x => x.Id == id);
        if (status == null)
        {
            throw new NotFoundException("Không tìm thấy trạng thái cuộc thi");
        }
        _unitOfWork.GetRepository<ShowStatus>().DeleteAsync(status);
        await _unitOfWork.CommitAsync();
    }
}