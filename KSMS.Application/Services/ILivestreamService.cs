using KSMS.Domain.Dtos.Responses.Livestream;

namespace KSMS.Application.Services;

public interface ILivestreamService
{
    Task CreateLivestream(Guid koiShowId, string streamUrl);
    Task EndLivestream(Guid id);
    Task<List<GetLiveStreamResponse>> GetLivestreams(Guid koiShowId);
    Task<GetLiveStreamResponse> GetLivestreamById(Guid id);
}