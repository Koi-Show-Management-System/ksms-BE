using System.Net.Http.Json;
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace KSMS.Infrastructure.Services;

public class LivestreamService : BaseService<LivestreamService>, ILivestreamService
{
    private readonly INotificationService _notificationService;
    private readonly IHubContext<LivestreamHub> _livestreamHub;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _apiSecret;
    private readonly string _baseUrl = "https//api.getstream.io/video/v1";
    public LivestreamService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<LivestreamService> logger, IHttpContextAccessor httpContextAccessor, INotificationService notificationService, IHubContext<LivestreamHub> livestreamHub, IConfiguration configuration, HttpClient httpClient) : base(unitOfWork, logger, httpContextAccessor)
    {
        _notificationService = notificationService;
        _livestreamHub = livestreamHub;
        _httpClient = httpClient;
        _apiKey = configuration["GetStream:ApiKey"];
        _apiSecret = configuration["GetStream:ApiSecret"];
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiSecret}");
    }

    public async Task CreateLivestream(Guid koiShowId, string streamUrl)
    {
        var show = await _unitOfWork.GetRepository<KoiShow>()
            .SingleOrDefaultAsync(predicate: s => s.Id == koiShowId);
        if (show == null)
        {
            throw new NotFoundException("Không tìm thấy triển lãm");
        }

        var callId = $"livestream_{Guid.NewGuid()}";
        var createCallRequest = new
        {
            Id = callId,
            type = "livestream",
            member = new[] { GetIdFromJwt().ToString() }
        };
        var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/{_apiKey}/call", createCallRequest);
        response.EnsureSuccessStatusCode();
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
        //if (!string.IsNullOrEmpty(livestream.))
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