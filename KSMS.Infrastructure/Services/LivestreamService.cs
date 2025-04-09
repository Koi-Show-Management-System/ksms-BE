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
    private readonly string _baseUrl = "https://api.getstream.io/video/v1";
    public LivestreamService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<LivestreamService> logger, IHttpContextAccessor httpContextAccessor, INotificationService notificationService, IHubContext<LivestreamHub> livestreamHub, IConfiguration configuration, HttpClient httpClient) : base(unitOfWork, logger, httpContextAccessor)
    {
        _notificationService = notificationService;
        _livestreamHub = livestreamHub;
        _httpClient = httpClient;
        _apiKey = configuration["GetStream:ApiKey"];
        _apiSecret = configuration["GetStream:ApiSecret"];
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiSecret}");
    }

    public async Task<object> CreateLivestream(Guid koiShowId)
    {
        // Kiểm tra triển lãm tồn tại
        var show = await _unitOfWork.GetRepository<KoiShow>()
            .SingleOrDefaultAsync(predicate: s => s.Id == koiShowId);
        if (show == null)
        {
            throw new NotFoundException("Không tìm thấy triển lãm");
        }

        // Tạo ID cho livestream
        var livestreamId = Guid.NewGuid();
        var callId = $"livestream_{livestreamId}";
        
        // Chuẩn bị request để tạo call trên getstream.io
        var createCallRequest = new
        {
            user_id = GetIdFromJwt().ToString(),
            type = "livestream",
            members = new[] { new { user_id = GetIdFromJwt().ToString(), role = "broadcaster" } }
        };
        
        // Gọi API để tạo call
        var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/{_apiKey}/call", createCallRequest);
        response.EnsureSuccessStatusCode();
        
        // Lưu thông tin livestream vào DB
        var livestream = new Livestream
        {
            Id = livestreamId,
            KoiShowId = show.Id,
            StreamUrl = callId,
            StartTime = VietNamTimeUtil.GetVietnamTime()
        };
        
        await _unitOfWork.GetRepository<Livestream>().InsertAsync(livestream);
        await _unitOfWork.CommitAsync();
        
        // Thông báo qua SignalR
        await _livestreamHub.Clients.All.SendAsync("NewLivestream", new
        {
            Id = livestreamId,
            ShowId = koiShowId,
            ShowName = show.Name,
            StreamUrl = callId,
            StartTime = VietNamTimeUtil.GetVietnamTime()
        });
        return new
        {
            Id = livestreamId
        };
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
        var callId = $"livestream_{id}";
        var response = await _httpClient.DeleteAsync($"{_baseUrl}/{_apiKey}/call/{callId}");
        response.EnsureSuccessStatusCode();
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

    public async Task<TokenResponse?> GetLiveStreamViewToken(Guid id)
    {
        var livestream = await _unitOfWork.GetRepository<Livestream>()
            .SingleOrDefaultAsync(predicate: l => l.Id == id);
        if (livestream == null)
        {
            throw new NotFoundException("Không tìm thấy livestream");
        }

        string userId;
        var isAuthenticated = _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
        userId = isAuthenticated ? GetIdFromJwt().ToString() : $"guest_{Guid.NewGuid()}";
        var createTokenRequest = new
        {
            user_id = userId,
            call_id = $"livestream_{id}",
            role = "viewer",
        };
        var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/{_apiKey}/token", createTokenRequest);
        response.EnsureSuccessStatusCode();
        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return tokenResponse;
    }
    public async Task<TokenResponse?> GetLiveStreamHostToken(Guid id)
    {
        var livestream = await _unitOfWork.GetRepository<Livestream>()
            .SingleOrDefaultAsync(predicate: l => l.Id == id);
        if (livestream == null)
        {
            throw new NotFoundException("Không tìm thấy livestream");
        }

        var createTokenRequest = new
        {
            user_id = GetIdFromJwt().ToString(),
            call_id = $"livestream_{id}",
            role = "broadcaster",
        };
        var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/{_apiKey}/token", createTokenRequest);
        response.EnsureSuccessStatusCode();
        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return tokenResponse;
    }
}