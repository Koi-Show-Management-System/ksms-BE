using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.ShowStatus;
using KSMS.Domain.Entities;
using KSMS.Domain.Exceptions;
using KSMS.Infrastructure.Database;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace KSMS.Infrastructure.Services;

public class ShowStatusService : BaseService<ShowStatusService>, IShowStatusService
{
    public ShowStatusService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<ShowStatusService> logger, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, httpContextAccessor)
    {
    }

    public async Task CreateShowStatusAsync(Guid koiShowId, List<CreateShowStatusRequest> requests)
    {
        var show = await _unitOfWork.GetRepository<KoiShow>().SingleOrDefaultAsync(predicate: x => x.Id == koiShowId);
        if (show == null)
        {
            throw new NotFoundException("Không tìm thấy cuộc thi");
        }

        var statusList = new List<ShowStatus>();
        foreach (var request in requests)
        {
            var status = request.Adapt<ShowStatus>();
            status.KoiShowId = koiShowId;
            statusList.Add(status);
        }

        await _unitOfWork.GetRepository<ShowStatus>().InsertRangeAsync(statusList);
        await _unitOfWork.CommitAsync();
    }

    public async Task UpdateShowStatusAsync(Guid koiShowId, List<UpdateShowStatusRequestV2> requests)
    {
        var show = await _unitOfWork.GetRepository<KoiShow>().SingleOrDefaultAsync(predicate: x => x.Id == koiShowId);
        if (show == null)
        {
            throw new NotFoundException("Không tìm thấy cuộc thi");
        }

        // Lấy tất cả các trạng thái hiện có của cuộc thi
        var existingStatuses = await _unitOfWork.GetRepository<ShowStatus>().GetListAsync(predicate: x => x.KoiShowId == koiShowId);
        
        // Xóa tất cả các trạng thái hiện có
        _unitOfWork.GetRepository<ShowStatus>().DeleteRangeAsync(existingStatuses);
        
        // Tạo danh sách các trạng thái mới
        var newStatusList = new List<ShowStatus>();
        foreach (var request in requests)
        {
            var status = request.Adapt<ShowStatus>();
            status.KoiShowId = koiShowId;
            newStatusList.Add(status);
        }
        
        // Thêm các trạng thái mới
        await _unitOfWork.GetRepository<ShowStatus>().InsertRangeAsync(newStatusList);
        
        // Lưu thay đổi
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