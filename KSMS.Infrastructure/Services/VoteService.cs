using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Responses.KoiMedium;
using KSMS.Domain.Dtos.Responses.Vote;
using KSMS.Domain.Entities;
using KSMS.Domain.Enums;
using KSMS.Domain.Exceptions;
using KSMS.Domain.Pagination;
using KSMS.Infrastructure.Database;
using KSMS.Infrastructure.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KSMS.Infrastructure.Services;

public class VoteService : BaseService<VoteService>, IVoteService
{
    private readonly INotificationService _notificationService;
    private readonly IHubContext<VoteHub> _voteHub;
    public VoteService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<VoteService> logger, IHttpContextAccessor httpContextAccessor, INotificationService notificationService, IHubContext<VoteHub> voteHub) : base(unitOfWork, logger, httpContextAccessor)
    {
        _notificationService = notificationService;
        _voteHub = voteHub;
    }

    public async Task CreateVote(Guid registrationId)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var accountId = GetIdFromJwt();
            var registration = await _unitOfWork.GetRepository<Registration>()
            .SingleOrDefaultAsync(
                predicate: r => r.Id == registrationId,
                include: query => query
                    .Include(r => r.KoiShow)
                    .Include(r => r.RegistrationRounds)
                    .ThenInclude(rr => rr.Round));
            if (registration == null)
            {
                throw new NotFoundException("Không tìm thấy thông tin cá");
            }
            if (!registration.KoiShow.EnableVoting)
            {
                throw new BadRequestException("Chức năng bình chọn chưa được kích hoạt");
            }

            var highestFinalRounds = await GetHighestFinalRounds(registration.KoiShowId);
            if (!highestFinalRounds.Any())
            {
                throw new BadRequestException("Chưa có vòng chung kết nào");
            }
            if (!registration.RegistrationRounds.Any(rr => highestFinalRounds.Select(fr => fr.Id).Contains(rr.RoundId)))
            {
                throw new BadRequestException("Cá này không nằm trong vòng chung kết");
            }
            var hasCheckinTicket = await _unitOfWork.GetRepository<Ticket>()
                .GetListAsync( predicate:t =>
                        t.TicketOrderDetail.TicketOrder.AccountId == accountId &&
                        t.TicketOrderDetail.TicketType.KoiShowId == registration.KoiShowId &&
                        t.Status == TicketStatus.Checkin.ToString().ToLower(),
                    include: query => query
                        .Include(t => t.TicketOrderDetail)
                        .ThenInclude(tod => tod.TicketOrder)
                        .Include(t => t.TicketOrderDetail)
                        .ThenInclude(tod => tod.TicketType));
            if (!hasCheckinTicket.Any())
            {
                throw new BadRequestException("Bạn cần check-in vé để tham gia bình chọn");
            }
            var existingVote = await _unitOfWork.GetRepository<Vote>()
                .SingleOrDefaultAsync(predicate: v => v.RegistrationId == registrationId && v.AccountId == accountId);
            if (existingVote != null)
            {
                throw new BadRequestException("Bạn đã bình chọn cho cá này");
            }
            var hasVotedInShow = await _unitOfWork.GetRepository<Vote>()
                .GetListAsync(predicate: v => v.AccountId == accountId && 
                                              v.Registration.KoiShowId == registration.KoiShowId,
                    include: query => query.Include(v => v.Registration));
            if (hasVotedInShow.Any())
            {
                throw new BadRequestException("Bạn chỉ được bình chọn một lần và không thể thay đổi bình chọn");
            }
            var vote = new Vote
            {
                AccountId = accountId,
                RegistrationId = registrationId
            };
            await _unitOfWork.GetRepository<Vote>().InsertAsync(vote);
            await _unitOfWork.CommitAsync();
            var actualVoteCount = await _unitOfWork.GetRepository<Vote>()
                .CountAsync(predicate: v => v.RegistrationId == registrationId);
            await _voteHub.Clients.All.SendAsync("ReceiveVoteUpdate", new
            {
                RegistrationId = registrationId,
                VoteCount = actualVoteCount
            });
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task EnableVoting(Guid showId)
    {
        var show = await _unitOfWork.GetRepository<KoiShow>()
            .SingleOrDefaultAsync(predicate: s => s.Id == showId);
        if (show == null)
        {
            throw new NotFoundException("Không tìm thấy triển lãm");
        }
        var highestFinalRounds = await GetHighestFinalRounds(showId);
        if (!highestFinalRounds.Any())
        {
            throw new BadRequestException("Chưa có vòng chung kết nào");
        }
        
        var unfinishedRegistrationRounds = await _unitOfWork.GetRepository<RegistrationRound>()
            .GetListAsync(
                predicate: rr =>
                    highestFinalRounds.Select(r => r.Id).Contains(rr.RoundId) &&
                    !rr.RoundResults.Any(),
                include: query => query
                    .Include(rr => rr.Round)
                    .Include(rr => rr.RoundResults)
            );
        if (unfinishedRegistrationRounds.Any())
        {
            throw new BadRequestException("Có vài vòng chung kết chưa hoàn thành, không thể kích hoạt chức năng bình chọn");
        }
        show.EnableVoting = true;
        _unitOfWork.GetRepository<KoiShow>().UpdateAsync(show);
        await _unitOfWork.CommitAsync();
        var memberIds = await _unitOfWork.GetRepository<Account>()
            .GetListAsync(
                selector: a => a.Id,
                predicate: a => a.Role == RoleName.Member.ToString().ToLower());
            
        // Gửi thông báo đến tất cả member
        await _notificationService.SendNotificationToMany(
            memberIds.ToList(),
            "Bình chọn cá yêu thích đã mở",
            $"Tính năng bình chọn cá yêu thích cho triển lãm {show.Name} đã được kích hoạt. Hãy tham gia bình chọn ngay! Lưu ý: Bạn cần check-in vé để có thể tham gia bình chọn.",
            Domain.Enums.NotificationType.System);
    }

    public async Task DisableVoting(Guid showId)
    {
        var show = await _unitOfWork.GetRepository<KoiShow>()
            .SingleOrDefaultAsync(predicate: s => s.Id == showId);
        if (show == null)
        {
            throw new NotFoundException("Không tìm thấy triển lãm");
        }
        
        // Vô hiệu hóa bình chọn
        show.EnableVoting = false;
        _unitOfWork.GetRepository<KoiShow>().UpdateAsync(show);
        await _unitOfWork.CommitAsync();
        
        // Kiểm tra xem có vote nào không trước khi lấy kết quả
        var hasVotes = await _unitOfWork.GetRepository<Vote>()
            .GetListAsync(
                predicate: v => v.Registration.KoiShowId == showId,
                include: q => q.Include(v => v.Registration));
        
        if (!hasVotes.Any())
        {
            // Nếu không có vote nào, chỉ vô hiệu hóa bình chọn mà không gửi thông báo
            return;
        }
        
        // Có votes, lấy kết quả và gửi thông báo
        try 
        {
            var votingResults = await GetVotingResults(showId);
            if (votingResults.Any())
            {
                // Tìm số vote cao nhất
                int maxVotes = votingResults.Max(r => r.VoteCount);
                
                // Nếu có ít nhất 1 vote
                if (maxVotes > 0)
                {
                    // Lấy tất cả cá có số vote cao nhất
                    var winners = votingResults.Where(r => r.VoteCount == maxVotes).ToList();
                    
                    foreach (var winner in winners)
                    {
                        var registration = await _unitOfWork.GetRepository<Registration>()
                            .SingleOrDefaultAsync(predicate: r => r.Id == winner.RegistrationId,
                                include: query => query
                                    .Include(r => r.KoiProfile)
                                    .ThenInclude(k => k.Owner));
                                    
                        if (registration != null)
                        {
                            string message;
                            
                            if (winners.Count > 1)
                            {
                                // Tin nhắn cho trường hợp hòa
                                message = $"Chúc mừng! Koi {registration.KoiProfile.Name} của bạn đã nhận được {winner.VoteCount} lượt bình chọn từ khán giả và đồng đạt giải People's Choice Award trong triển lãm {show.Name}. Có {winners.Count} cá cùng đạt giải với cùng số lượng bình chọn.";
                            }
                            else
                            {
                                // Tin nhắn cho trường hợp chiến thắng độc lập
                                message = $"Chúc mừng! Koi {registration.KoiProfile.Name} của bạn đã nhận được {winner.VoteCount} lượt bình chọn từ khán giả và trở thành con cá được yêu thích nhất trong triển lãm {show.Name}";
                            }
                            
                            await _notificationService.SendNotification(
                                registration.KoiProfile.OwnerId,
                                "Kết quả People's Choice Award",
                                message,
                                NotificationType.System);
                        }
                    }
                }
            }
        }
        catch (BadRequestException)
        {
            // Bắt exception từ GetVotingResults nếu có, nhưng không làm gián đoạn quá trình disable voting
            // Đã kiểm tra hasVotes.Any() ở trên, nên về lý thuyết không vào đây
        }
    }

    public async Task<List<GetFinalRegistrationResponse>> GetFinalRegistration(Guid showId)
    {
        var show = await _unitOfWork.GetRepository<KoiShow>()
            .SingleOrDefaultAsync(predicate: s => s.Id == showId);
        if (show == null)
        {
            throw new NotFoundException("Không tìm thấy triển lãm");
        }
        if (!show.EnableVoting)
        {
            throw new BadRequestException("Chức năng bình chọn chưa được kích hoạt");
        }
        var highestFinalRounds = await GetHighestFinalRounds(showId);
        if (!highestFinalRounds.Any())
        {
            throw new BadRequestException("Chưa có vòng chung kết nào");
        }
        var unfinishedRegistrationRounds = await _unitOfWork.GetRepository<RegistrationRound>()
            .GetListAsync(
                predicate: rr =>
                    highestFinalRounds.Select(r => r.Id).Contains(rr.RoundId) &&
                    !rr.RoundResults.Any(),
                include: query => query
                    .Include(rr => rr.Round)
                    .Include(rr => rr.RoundResults)
            );
        if (unfinishedRegistrationRounds.Any())
        {
            throw new BadRequestException("Có vài vòng chung kết chưa hoàn thành, không thể kích hoạt chức năng bình chọn");
        }

        var registrations = await _unitOfWork.GetRepository<Registration>()
            .GetListAsync(
                predicate: r => r.KoiShowId == showId &&
                                r.RegistrationRounds.Any(rr =>
                                    highestFinalRounds.Select(fr => fr.Id).Contains(rr.RoundId)),
                include: query => query
                    .Include(r => r.KoiProfile)
                        .ThenInclude(r => r.Variety)
                    .Include(r => r.KoiProfile)
                        .ThenInclude(r => r.Owner)
                    .Include(r => r.CompetitionCategory)
                    .Include(r => r.KoiMedia)
                    .Include(r => r.RegistrationRounds)
                        .ThenInclude(rr => rr.Round)
                    .Include(r => r.RegistrationRounds)
                        .ThenInclude(rr => rr.Tank)
                    .Include(r => r.Votes));
        return registrations.Select(r => new GetFinalRegistrationResponse
        {
            RegistrationId = r.Id,
            RegistrationNumber = r.RegistrationNumber,
            RegisterName = r.RegisterName,
            CategoryName = r.CompetitionCategory.Name,
            KoiName = r.KoiProfile.Name,
            KoiVariety = r.KoiProfile.Variety.Name,
            Size = r.KoiSize,
            Age = r.KoiAge,
            Gender = r.KoiProfile.Gender,
            Bloodline = r.KoiProfile.Bloodline,
            OwnerName = r.KoiProfile.Owner.FullName,
            KoiMedia = r.KoiMedia.Select(km => new GetKoiMediaResponse
            {
                Id = km.Id,
                MediaUrl = km.MediaUrl,
                MediaType = km.MediaType,
            }).ToList(),
            RoundInfo = r.RegistrationRounds
                .Where(rr => highestFinalRounds.Select(fr => fr.Id).Contains(rr.RoundId))
                .Select(rr => new GetRoundInfoResponse
                {
                    TankNumber = rr.Tank?.Name
                }).FirstOrDefault()
        }).ToList();
    }

    public async Task<List<GetVotingResultResponse>> GetVotingResults(Guid showId)
    {
        var show = await _unitOfWork.GetRepository<KoiShow>()
            .SingleOrDefaultAsync(predicate: s => s.Id == showId);
        if (show == null)
        {
            throw new NotFoundException("Không tìm thấy triển lãm");
        }

        var hasVotes = await _unitOfWork.GetRepository<Vote>()
            .GetListAsync(predicate: v => v.Registration.KoiShowId == showId,
                include: q => q.Include(v => v.Registration));
        if (!hasVotes.Any())
        {
            throw new BadRequestException("Triển lãm chưa có giai đoạn bình chọn");
        }

        if (show.EnableVoting)
            throw new BadRequestException("Chưa kết thúc thời gian bình chọn");
        var highestFinalRounds = await GetHighestFinalRounds(showId);
        var votingResults =  await _unitOfWork.GetRepository<Registration>()
            .GetListAsync(
                selector: r => new GetVotingResultResponse
                {
                    RegistrationId = r.Id,
                    RegistrationNumber = r.RegistrationNumber,
                    RegisterName = r.RegisterName,
                    CategoryName = r.CompetitionCategory.Name,
                    KoiName = r.KoiProfile.Name,
                    KoiVariety = r.KoiProfile.Variety.Name,
                    Size = r.KoiSize,
                    Age = r.KoiAge,
                    Gender = r.KoiProfile.Gender,
                    Bloodline = r.KoiProfile.Bloodline,
                    OwnerName = r.Account.FullName,
                    KoiMedia = r.KoiMedia.Select(km => new GetKoiMediaResponse
                    {
                        Id = km.Id,
                        MediaUrl = km.MediaUrl,
                        MediaType = km.MediaType,
                    }).ToList(),
                    VoteCount = r.Votes.Count
                },
                predicate: r => r.KoiShowId == showId &&
                                r.RegistrationRounds.Any(rr =>
                                    highestFinalRounds.Select(fr => fr.Id).Contains(rr.RoundId)),
                orderBy: q => q.OrderByDescending(r => r.Votes.Count),
                include: query => query.AsSplitQuery()
                    .Include(r => r.KoiProfile)
                        .ThenInclude(r => r.Variety)
                    .Include(r => r.Account)
                    .Include(r => r.CompetitionCategory)
                    .Include(r => r.KoiMedia)
                    .Include(r => r.Votes)
                    .Include(r => r.RegistrationRounds)
                        .ThenInclude(rr => rr.Round)
                    .Include(r => r.KoiShow)
                        .ThenInclude(s => s.Registrations)
                            .ThenInclude(r => r.Votes)
            );
        var registrations = votingResults.ToList();
        if (registrations.Any())
        {
            var groupedByVotes = registrations
                .GroupBy(r => r.VoteCount)
                .OrderByDescending(g => g.Key)
                .ToList();
            int currentRank = 1;
            foreach (var group in groupedByVotes)
            {
                if (group.Key <= 0)
                    continue;
            
                foreach (var reg in group)
                {
                    reg.Rank = currentRank;
                    if (currentRank == 1 && !show.EnableVoting)
                    {
                        reg.Award = new GetFinalRegistrationResponse.AwardInfo
                        {
                            Name = "Giải bình chọn khán giả",
                            AwardType = "peoples_choice",
                            PrizeValue = null
                        };
                    }
                }
                currentRank++;
            }
        }

        return registrations;
    }

    public async Task<List<GetFinalRegistrationResponse>> GetVotingRegistrationsForStaff(Guid showId)
    {
        var show = await _unitOfWork.GetRepository<KoiShow>()
            .SingleOrDefaultAsync(predicate: s => s.Id == showId);
        
        if (show == null)
            throw new NotFoundException("Không tìm thấy triển lãm");
        var highestFinalRounds = await GetHighestFinalRounds(showId);

        var response=  await _unitOfWork.GetRepository<Registration>()
            .GetListAsync(
                selector: r => new GetFinalRegistrationResponse
                {
                    RegistrationId = r.Id,
                    RegistrationNumber = r.RegistrationNumber,
                    RegisterName = r.RegisterName,
                    CategoryName = r.CompetitionCategory.Name,
                    KoiName = r.KoiProfile.Name,
                    KoiVariety = r.KoiProfile.Variety.Name,
                    Size = r.KoiSize,
                    Age = r.KoiAge,
                    Gender = r.KoiProfile.Gender,
                    Bloodline = r.KoiProfile.Bloodline,
                    OwnerName = r.Account.FullName,
                    KoiMedia = r.KoiMedia.Select(km => new GetKoiMediaResponse()
                    {
                        MediaUrl = km.MediaUrl,
                        MediaType = km.MediaType
                    }).ToList(),
                    RoundInfo = r.RegistrationRounds
                        .Where(rr => highestFinalRounds.Select(fr => fr.Id).Contains(rr.RoundId))
                        .Select(rr => new GetRoundInfoResponse()
                        {
                            TankNumber = rr.Tank.Name,
                        }).FirstOrDefault(),
                    VoteCount = r.Votes.Count,
                },
                predicate: r => r.KoiShowId == showId && 
                    r.RegistrationRounds.Any(rr => highestFinalRounds.Select(fr => fr.Id).Contains(rr.RoundId)),
                orderBy: q => q.OrderByDescending(r => r.Votes.Count),
                include: query => query.AsSplitQuery()
                    .Include(r => r.KoiProfile)
                        .ThenInclude(k => k.Variety)
                    .Include(r => r.Account)
                    .Include(r => r.CompetitionCategory)
                    .Include(r => r.KoiMedia)
                    .Include(r => r.Votes)
                    .Include(r => r.RegistrationRounds)
                        .ThenInclude(rr => rr.Round)
                    .Include(r => r.RegistrationRounds)
                        .ThenInclude(rr => rr.Tank)
            );
        
        var registrations = response.ToList();
        if (registrations.Any() && !show.EnableVoting)
        {
            var maxVotes = registrations.Max(r => r.VoteCount);
            if (maxVotes <= 0) return registrations;
            {
                foreach (var reg in registrations.Where(r => r.VoteCount == maxVotes))
                {
                    reg.Award = new GetFinalRegistrationResponse.AwardInfo
                    {
                        Name = "Giải bình chọn khán giả",
                        AwardType = "peoples_choice",
                        PrizeValue = null
                    };
                }
            }
        }

        return registrations;
    }

    private async Task<List<Round>> GetHighestFinalRounds(Guid showId)
    {
        var categories = await _unitOfWork.GetRepository<CompetitionCategory>()
            .GetListAsync(predicate: cc => cc.KoiShowId == showId,
                include: q => q.Include(cc => cc.Rounds));
        var highestFinalRounds = new List<Round>();
        foreach (var category in categories)
        {
            var finalRounds = category.Rounds
                .Where(r => r.RoundType == RoundEnum.Final.ToString())
                .OrderByDescending(r => r.RoundOrder);
            var highestFinalRound = finalRounds.FirstOrDefault();
            if (highestFinalRound != null)
            {
                highestFinalRounds.Add(highestFinalRound);
            }
        }

        return highestFinalRounds;
    }
}