using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KSMS.Domain.Dtos.Responses.Livestream;

namespace KSMS.Application.Services;

public interface ILivestreamService
{
    Task<LivestreamTokenResponse> CreateLivestream(Guid koiShowId);
    Task<TokenResponse?> GetLiveStreamViewToken(Guid id);
    Task<TokenResponse?> GetLiveStreamHostToken(Guid id);
    Task EndLivestream(Guid id);
    Task StartLivestream(Guid id);
    Task<List<GetLiveStreamResponse>> GetLivestreams(Guid koiShowId);
    Task<GetLiveStreamResponse> GetLivestreamById(Guid id);
}