using KSMS.Domain.Dtos.Responses.Vote;
using KSMS.Domain.Pagination;

namespace KSMS.Application.Services;

public interface IVoteService
{
    Task CreateVote(Guid registrationId);
    Task EnableVoting(Guid showId);
    Task DisableVoting(Guid showId);
    Task<List<GetFinalRegistrationResponse>> GetFinalRegistration(Guid showId);
    Task<List<GetVotingResultResponse>> GetVotingResults(Guid showId);
    Task<List<GetFinalRegistrationResponse>> GetVotingRegistrationsForStaff(Guid showId);
}