using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
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
    private readonly string _baseUrl = "https://video.stream-io-api.com/api/v2";
    public LivestreamService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<LivestreamService> logger, IHttpContextAccessor httpContextAccessor, INotificationService notificationService, IHubContext<LivestreamHub> livestreamHub, IConfiguration configuration, HttpClient httpClient) : base(unitOfWork, logger, httpContextAccessor)
    {
        _notificationService = notificationService;
        _livestreamHub = livestreamHub;
        _httpClient = httpClient;
        _apiKey = configuration["GetStream:ApiKey"];
        _apiSecret = configuration["GetStream:ApiSecret"];
        
        // Tạo JWT server token và thiết lập headers
        SetAuthorizationHeaders();
    }
    
    private void SetAuthorizationHeaders()
    {
        // Tạo JWT server token
        var serverToken = GenerateServerToken();
        
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("stream-auth-type", "jwt");
        _httpClient.DefaultRequestHeaders.Add("Authorization", serverToken);
        
        _logger.LogInformation("Set up authorization headers with server token");
    }
    
    private string GenerateServerToken()
    {
        // Tạo JWT token với claim server=true
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_apiSecret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
        var claims = new[]
        {
            new Claim("server", "true")
        };
        
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: credentials
        );
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.WriteToken(token);
        
        _logger.LogInformation("Generated server token for GetStream API");
        return jwtToken;
    }
    
    // Tạo user token JWT
    private string GenerateUserToken(string userId, int expirationInSeconds = 3600, string[] callCids = null, string role = null)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_apiSecret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
        var claims = new List<Claim>
        {
            new Claim("user_id", userId),
        };
        
        if (callCids != null && callCids.Length > 0)
        {
            claims.Add(new Claim("call_cids", System.Text.Json.JsonSerializer.Serialize(callCids)));
        }
        
        if (!string.IsNullOrEmpty(role))
        {
            claims.Add(new Claim("role", role));
        }
        
        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            claims: claims,
            notBefore: now,
            expires: now.AddSeconds(expirationInSeconds),
            signingCredentials: credentials
        );
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.WriteToken(token);
        
        _logger.LogInformation($"Generated user token for user {userId}");
        return jwtToken;
    }

    public async Task<LivestreamTokenResponse> CreateLivestream(Guid koiShowId)
    {
        var show = await _unitOfWork.GetRepository<KoiShow>()
            .SingleOrDefaultAsync(predicate: s => s.Id == koiShowId);
        if (show == null)
        {
            throw new NotFoundException("Không tìm thấy triển lãm");
        }
        
        // Sử dụng định dạng ID đơn giản hơn
        var callId = Guid.NewGuid().ToString("N");
        var userId = GetIdFromJwt().ToString();
        
        try 
        {
            _logger.LogInformation($"Creating livestream call with ID: {callId}");
            
            // 1. Trước tiên, đảm bảo người dùng tồn tại trong GetStream
            var createUserRequest = new
            {
                users = new Dictionary<string, object>
                {
                    [userId] = new
                    {
                        id = userId,
                        role = "user",
                        name = userId
                    }
                }
            };
            
            var userResponse = await _httpClient.PostAsJsonAsync(
                $"{_baseUrl}/users?api_key={_apiKey}", 
                createUserRequest);
            
            if (!userResponse.IsSuccessStatusCode)
            {
                var errorContent = await userResponse.Content.ReadAsStringAsync();
                _logger.LogWarning($"Warning creating user: {userResponse.StatusCode}, Content: {errorContent}");
                // Tiếp tục thực hiện, có thể người dùng đã tồn tại
            }
            
            // 2. Tạo yêu cầu cho video call theo định dạng mới
            var createCallRequest = new
            {
                data = new
                {
                    created_by_id = userId,
                    members = new[]
                    {
                        new { role = "host", user_id = userId }
                    },
                    custom = new { }
                }
            };
            
            // Gọi API tạo call với định dạng endpoint mới
            var response = await _httpClient.PostAsJsonAsync(
                $"{_baseUrl}/video/call/livestream/{callId}?api_key={_apiKey}", 
                createCallRequest);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error creating livestream call: {response.StatusCode}, Error: {errorContent}");
                throw new Exception($"Failed to create call: {errorContent}");
            }
            
            // Lưu thông tin livestream vào DB
            var livestream = new Livestream
            {
                Id = Guid.NewGuid(),
                KoiShowId = show.Id,
                StreamUrl = callId,
                StartTime = VietNamTimeUtil.GetVietnamTime()
            };
            
            await _unitOfWork.GetRepository<Livestream>().InsertAsync(livestream);
            await _unitOfWork.CommitAsync();
            
            // 3. Tạo user token cho người dùng này
            // Tạo JWT token trực tiếp thay vì gọi API endpoint
            var callCids = new[] { $"livestream:{callId}" };
            var userToken = GenerateUserToken(userId, 24 * 60 * 60, callCids, "host");
            
            return new LivestreamTokenResponse
            {
                Id = livestream.Id,
                CallId = callId,
                Token = userToken
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating livestream");
            throw;
        }
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
        
        try 
        {
            // Sử dụng endpoint mới để kết thúc cuộc gọi
            var response = await _httpClient.DeleteAsync(
                $"{_baseUrl}/video/call/livestream/{livestream.StreamUrl}?api_key={_apiKey}");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error ending livestream: {response.StatusCode}, Error: {errorContent}");
                throw new Exception($"Failed to end livestream: {errorContent}");
            }
            
            livestream.EndTime = VietNamTimeUtil.GetVietnamTime(); 
            _unitOfWork.GetRepository<Livestream>().UpdateAsync(livestream);
            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending livestream");
            throw;
        }
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

        try
        {
            string userId;
            var isAuthenticated = _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
            userId = isAuthenticated ? GetIdFromJwt().ToString() : $"guest_{Guid.NewGuid()}";
            
            // Nếu là guest, hãy tạo user
            if (!isAuthenticated)
            {
                var createUserRequest = new
                {
                    users = new Dictionary<string, object>
                    {
                        [userId] = new
                        {
                            id = userId,
                            role = "user",
                            name = "Khách"
                        }
                    }
                };
                
                var userResponse = await _httpClient.PostAsJsonAsync(
                    $"{_baseUrl}/users?api_key={_apiKey}", 
                    createUserRequest);
                
                if (!userResponse.IsSuccessStatusCode)
                {
                    var errorContent = await userResponse.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Warning creating guest user: {userResponse.StatusCode}, Content: {errorContent}");
                    // Tiếp tục thực hiện, có thể guest user đã tồn tại
                }
                
                // Thêm khách vào cuộc gọi livestream với vai trò user
                var addMemberRequest = new
                {
                    update_members = new[]
                    {
                        new { user_id = userId, role = "user" }
                    }
                };
                
                await _httpClient.PostAsJsonAsync(
                    $"{_baseUrl}/video/call/livestream/{livestream.StreamUrl}/members?api_key={_apiKey}",
                    addMemberRequest);
            }
            
            // Tạo JWT token trực tiếp
            var callCids = new[] { $"livestream:{livestream.StreamUrl}" };
            var userToken = GenerateUserToken(userId, 3600, callCids, "user");
            
            return new TokenResponse { Token = userToken };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting view token");
            throw;
        }
    }
    
    public async Task<TokenResponse?> GetLiveStreamHostToken(Guid id)
    {
        var livestream = await _unitOfWork.GetRepository<Livestream>()
            .SingleOrDefaultAsync(predicate: l => l.Id == id);
        if (livestream == null)
        {
            throw new NotFoundException("Không tìm thấy livestream");
        }

        try
        {
            var userId = GetIdFromJwt().ToString();
            
            // Đảm bảo người dùng tồn tại trong GetStream
            var createUserRequest = new
            {
                users = new Dictionary<string, object>
                {
                    [userId] = new
                    {
                        id = userId,
                        role = "user",
                        name = userId
                    }
                }
            };
            
            await _httpClient.PostAsJsonAsync(
                $"{_baseUrl}/users?api_key={_apiKey}", 
                createUserRequest);
            
            // Thêm người dùng vào cuộc gọi livestream với vai trò host nếu chưa có
            var addMemberRequest = new
            {
                update_members = new[]
                {
                    new { user_id = userId, role = "host" }
                }
            };
            
            await _httpClient.PostAsJsonAsync(
                $"{_baseUrl}/video/call/livestream/{livestream.StreamUrl}/members?api_key={_apiKey}",
                addMemberRequest);
            
            // Tạo JWT token trực tiếp
            var callCids = new[] { $"livestream:{livestream.StreamUrl}" };
            var userToken = GenerateUserToken(userId, 3600, callCids, "host");
            
            return new TokenResponse { Token = userToken };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting host token");
            throw;
        }
    }
}