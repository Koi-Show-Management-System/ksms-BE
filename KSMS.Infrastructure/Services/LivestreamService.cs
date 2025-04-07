using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Responses.Livestream;
using KSMS.Domain.Entities;
using KSMS.Domain.Exceptions;
using KSMS.Infrastructure.Database;
using KSMS.Infrastructure.Hubs;
using KSMS.Infrastructure.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KSMS.Infrastructure.Services;

public class LivestreamService : BaseService<LivestreamService>, ILivestreamService
{
    private readonly INotificationService _notificationService;
    private readonly IHubContext<LivestreamHub> _livestreamHub;
    public LivestreamService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<LivestreamService> logger, IHttpContextAccessor httpContextAccessor, INotificationService notificationService, IHubContext<LivestreamHub> livestreamHub) : base(unitOfWork, logger, httpContextAccessor)
    {
        _notificationService = notificationService;
        _livestreamHub = livestreamHub;
    }

    public async Task CreateLivestream(Guid koiShowId, string streamUrl)
    {
        var show = await _unitOfWork.GetRepository<KoiShow>()
            .SingleOrDefaultAsync(predicate: s => s.Id == koiShowId);
        if (show == null)
        {
            throw new NotFoundException("Không tìm thấy triển lãm");
        }

        await _unitOfWork.GetRepository<Livestream>().InsertAsync(new Livestream
        {
            KoiShowId = show.Id,
            StreamUrl = streamUrl,
            StartTime = VietNamTimeUtil.GetVietnamTime()
        });
        await _unitOfWork.CommitAsync();
    }

    public async Task EndLivestream(Guid id)
    {
        var livestream = await _unitOfWork.GetRepository<Livestream>()
            .SingleOrDefaultAsync(predicate: s => s.Id == id,
                include: q => q.Include(l => l.KoiShow));
        if (livestream == null)
        {
            throw new NotFoundException("Không tìm thấy livestream");
        }
        livestream.EndTime = VietNamTimeUtil.GetVietnamTime(); 
        _unitOfWork.GetRepository<Livestream>().UpdateAsync(livestream);
        await _unitOfWork.CommitAsync();
    }

    public async Task<List<GetLiveStreamResponse>> GetLivestreams(Guid koiShowId)
    {
        var livestreams = await _unitOfWork.GetRepository<Livestream>()
            .GetListAsync(
                predicate: l => l.KoiShowId == koiShowId,
                include: query => query.Include(l => l.KoiShow),
                selector: l => new GetLiveStreamResponse
                {
                    Id = l.Id,
                    StreamUrl = l.StreamUrl,
                    StartTime = l.StartTime,
                    EndTime = l.EndTime,
                    ShowName = l.KoiShow.Name,
                    ViewerCount = LivestreamHub.GetCurrentViewerCount(l.Id.ToString())
                });
        return livestreams.ToList();
    }

    public async Task<GetLiveStreamResponse> GetLivestreamById(Guid id)
    {
        var livestream = await _unitOfWork.GetRepository<Livestream>()
            .SingleOrDefaultAsync(
                predicate: l => l.Id == id,
                include: query => query.Include(l => l.KoiShow),
                selector: l => new GetLiveStreamResponse
                {
                    Id = l.Id,
                    StreamUrl = l.StreamUrl,
                    StartTime = l.StartTime,
                    EndTime = l.EndTime,
                    ShowName = l.KoiShow.Name,
                    ViewerCount = LivestreamHub.GetCurrentViewerCount(l.Id.ToString())
                });
        return livestream;
    }
}