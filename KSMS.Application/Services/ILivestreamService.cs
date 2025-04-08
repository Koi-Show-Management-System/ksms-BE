using KSMS.Domain.Dtos.Responses.Livestream;

namespace KSMS.Application.Services;

public interface ILivestreamService
{
    Task<object> CreateLivestream(Guid koiShowId, string streamUrl);
    Task EndLivestream(Guid id);
    Task<List<GetLiveStreamResponse>> GetLivestreams(Guid koiShowId);
    Task<GetLiveStreamResponse> GetLivestreamById(Guid id);
    Task<TokenResponse?> GetLiveStreamViewToken(Guid id);
    Task<TokenResponse?> GetLiveStreamHostToken(Guid id);
}