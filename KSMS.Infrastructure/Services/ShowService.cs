using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.Show;
using KSMS.Domain.Dtos.Responses.KoiShow;

using KSMS.Domain.Entities;
using KSMS.Domain.Exceptions;
using KSMS.Infrastructure.Database;
using Mapster;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using KSMS.Infrastructure.Utils;
using Microsoft.AspNetCore.Http;
using static KSMS.Infrastructure.Utils.MailUtil;
using KSMS.Domain.Pagination;
using System.Linq.Expressions;
using KSMS.Application.Extensions;
using KSMS.Domain.Dtos;
using KSMS.Domain.Dtos.Requests.ShowRule;

namespace KSMS.Infrastructure.Services
{
    public class ShowService : BaseService<ShowService>, IShowService
    {

        public ShowService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork,
            ILogger<ShowService> logger, IHttpContextAccessor httpContextAccessor)
            : base(unitOfWork, logger, httpContextAccessor)
        {
        }

        public async Task UpdateShowV2(Guid id, UpdateShowRequestV2 request)
        {
            var show = await _unitOfWork.GetRepository<KoiShow>().SingleOrDefaultAsync(predicate: x => x.Id == id,
                include: query => query.Include(x => x.Sponsors)
                    .Include(x => x.TicketTypes)
                    .Include(x => x.ShowRules)
                    .Include(x => x.ShowStatuses));
            if (show is null)
            {
                throw new NotFoundException("Show is not existed");
            }
            request.Adapt(show);
            _unitOfWork.GetRepository<KoiShow>().UpdateAsync(show);
            await _unitOfWork.CommitAsync();
            
        }

        public async Task<GetKoiShowDetailResponse> GetShowDetailByIdAsync(Guid id)
        {
            var show = await _unitOfWork.GetRepository<KoiShow>().SingleOrDefaultAsync(predicate: x => x.Id == id,
                include: query => query.Include(x => x.ShowRules)
                    .Include(x => x.ShowStatuses)
                    .Include(x => x.Sponsors)
                    .Include(x => x.TicketTypes)
                    .Include(x => x.CompetitionCategories)
                    .ThenInclude(c => c.CriteriaCompetitionCategories)
                    .ThenInclude(cc => cc.Criteria));
            if (show is null)
            {
                throw new NotFoundException("Show is not existed");
            }
            var response = show.Adapt<GetKoiShowDetailResponse>();
            var criteria = show.CompetitionCategories
                .SelectMany(c => c.CriteriaCompetitionCategories
                    .Select(cc => cc.Criteria.Name)).Distinct().ToList();
            response.Criteria = criteria.ToList();
            return response;
        }
        
        public async Task CreateShowAsync(CreateShowRequest createShowRequest)
        {
            if (string.IsNullOrWhiteSpace(createShowRequest.Name))
                throw new BadRequestException("Show name cannot be empty.");

            if (createShowRequest.StartDate >= createShowRequest.EndDate)
                throw new BadRequestException("Start date must be earlier than end date.");
            var showRepository = _unitOfWork.GetRepository<KoiShow>();
            var categoryRepository = _unitOfWork.GetRepository<CompetitionCategory>();
            var sponsorRepository = _unitOfWork.GetRepository<Sponsor>();
            var showStaffRepository = _unitOfWork.GetRepository<ShowStaff>();
            var showRuleRepository = _unitOfWork.GetRepository<ShowRule>();
            var showStatusRepository = _unitOfWork.GetRepository<ShowStatus>();
            var roundRepository = _unitOfWork.GetRepository<Round>();
            var criteriaGroupRepository = _unitOfWork.GetRepository<CriteriaCompetitionCategory>();
            var criteriaRepository = _unitOfWork.GetRepository<Criterion>();
            var refereeAssignmentRepository = _unitOfWork.GetRepository<RefereeAssignment>();
            var accountRepository = _unitOfWork.GetRepository<Account>();
            var categoryVarietyRepository = _unitOfWork.GetRepository<CategoryVariety>();
            var awardRepository = _unitOfWork.GetRepository<Award>();
    
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try 
            {
                // Tạo show
                var newShow = createShowRequest.Adapt<KoiShow>();
                newShow.CreatedAt = DateTime.UtcNow;
                await showRepository.InsertAsync(newShow);
                await _unitOfWork.CommitAsync();
                var showId = newShow.Id;
                
                // Xử lý show rules
                if (createShowRequest.CreateShowRuleRequests.Any())
                {
                    var rules = createShowRequest.CreateShowRuleRequests
                        .Select(r => {
                            var rule = r.Adapt<ShowRule>();
                            rule.KoiShowId = showId;
                            return rule;
                        }).ToList();
                    
                    await showRuleRepository.InsertRangeAsync(rules);
                }
                
                // Xử lý show statuses
                if (createShowRequest.CreateShowStatusRequests.Any())
                {
                    var statuses = createShowRequest.CreateShowStatusRequests
                        .Select(s => {
                            var status = s.Adapt<ShowStatus>();
                            status.KoiShowId = showId;
                            return status;
                        }).ToList();
                    
                    await showStatusRepository.InsertRangeAsync(statuses);
                }
                
                // Xử lý sponsors
                if (createShowRequest.CreateSponsorRequests.Any())
                {
                    var sponsors = createShowRequest.CreateSponsorRequests
                        .Select(s => {
                            var sponsor = s.Adapt<Sponsor>();
                            sponsor.KoiShowId = showId;
                            return sponsor;
                        }).ToList();
                    
                    await sponsorRepository.InsertRangeAsync(sponsors);
                }
                
                // Xử lý ticket types
                if (createShowRequest.CreateTicketTypeRequests.Any())
                {
                    var ticketTypes = createShowRequest.CreateTicketTypeRequests
                        .Select(t => {
                            var ticket = t.Adapt<TicketType>();
                            ticket.KoiShowId = showId;
                            return ticket;
                        }).ToList();
                    
                    await _unitOfWork.GetRepository<TicketType>().InsertRangeAsync(ticketTypes);
                }
                
                // Xử lý staff assignments
                var currentUserId = GetIdFromJwt();
                var staffEmails = new List<(Guid id, string fullName, string email)>();
                
                if (createShowRequest.AssignStaffRequests.Any())
                {
                    var staffs = createShowRequest.AssignStaffRequests
                        .Select(staffId => new ShowStaff {
                            KoiShowId = showId,
                            AccountId = staffId,
                            AssignedBy = currentUserId,
                            AssignedAt = DateTime.UtcNow
                        }).ToList();
                    
                    await showStaffRepository.InsertRangeAsync(staffs);
                    
                    // Lấy thông tin staff để gửi email sau
                    var staffAccounts = await accountRepository.GetListAsync(
                        predicate: a => createShowRequest.AssignStaffRequests.Contains(a.Id));
                    
                    staffEmails.AddRange(staffAccounts.Select(a => (a.Id, a.FullName, a.Email)));
                }
                
                // Xử lý manager assignments
                if (createShowRequest.AssignManagerRequests.Any())
                {
                    var managers = createShowRequest.AssignManagerRequests
                        .Select(managerId => new ShowStaff {
                            KoiShowId = showId,
                            AccountId = managerId,
                            AssignedBy = currentUserId,
                            AssignedAt = DateTime.UtcNow
                        }).ToList();
                    
                    await showStaffRepository.InsertRangeAsync(managers);
                    
                    // Lấy thông tin manager để gửi email sau
                    var managerAccounts = await accountRepository.GetListAsync(
                        predicate: a => createShowRequest.AssignManagerRequests.Contains(a.Id));
                    
                    staffEmails.AddRange(managerAccounts.Select(a => (a.Id, a.FullName, a.Email)));
                }
                
                // Xử lý categories
                if (createShowRequest.CreateCategorieShowRequests.Any())
                {
                    foreach (var categoryRequest in createShowRequest.CreateCategorieShowRequests)
                    {
                        // Tạo category
                        var category = categoryRequest.Adapt<CompetitionCategory>();
                        category.KoiShowId = showId;
                        await categoryRepository.InsertAsync(category);
                        await _unitOfWork.CommitAsync();
                        // Xử lý varieties
                        if (categoryRequest.CreateCompetionCategoryVarieties.Any())
                        {
                            // Kiểm tra tất cả variety ID có tồn tại
                            var varieties = await _unitOfWork.GetRepository<Variety>().GetListAsync(
                                predicate: v => categoryRequest.CreateCompetionCategoryVarieties.Contains(v.Id));
                            
                            if (varieties.Count != categoryRequest.CreateCompetionCategoryVarieties.Count)
                            {
                                var foundIds = varieties.Select(v => v.Id).ToList();
                                var missingIds = categoryRequest.CreateCompetionCategoryVarieties
                                    .Where(id => !foundIds.Contains(id))
                                    .ToList();
                                throw new NotFoundException($"Varieties with IDs: {string.Join(", ", missingIds)} not found");
                            }
                            
                            var categoryVarieties = varieties.Select(v => new CategoryVariety {
                                CompetitionCategoryId = category.Id,
                                VarietyId = v.Id
                            }).ToList();
                            
                            await categoryVarietyRepository.InsertRangeAsync(categoryVarieties);
                        }
                        
                        // Xử lý criteria
                        if (categoryRequest.CreateCriteriaCompetitionCategoryRequests.Any())
                        {
                            var criteriaIds = categoryRequest.CreateCriteriaCompetitionCategoryRequests
                                .Select(c => c.CriteriaId)
                                .Distinct()
                                .ToList();
                            
                            var criteria = await criteriaRepository.GetListAsync(
                                predicate: c => criteriaIds.Contains(c.Id));
                            
                            if (criteria.Count != criteriaIds.Count)
                            {
                                var foundIds = criteria.Select(c => c.Id).ToList();
                                var missingIds = criteriaIds
                                    .Where(id => !foundIds.Contains(id))
                                    .ToList();
                                throw new BadRequestException($"Criteria with IDs: {string.Join(", ", missingIds)} not found");
                            }
                            
                            var criteriaGroups = categoryRequest.CreateCriteriaCompetitionCategoryRequests
                                .Select(g => {
                                    var group = g.Adapt<CriteriaCompetitionCategory>();
                                    group.CompetitionCategoryId = category.Id;
                                    return group;
                                }).ToList();
                            
                            await criteriaGroupRepository.InsertRangeAsync(criteriaGroups);
                        }
                        
                        // Xử lý rounds
                        if (categoryRequest.CreateRoundRequests.Any())
                        {
                            var rounds = categoryRequest.CreateRoundRequests
                                .Select(r => {
                                    var round = r.Adapt<Round>();
                                    round.CompetitionCategoriesId = category.Id;
                                    return round;
                                }).ToList();
                            
                            await roundRepository.InsertRangeAsync(rounds);
                        }
                        
                        // Xử lý referee assignments
                        if (categoryRequest.CreateRefereeAssignmentRequests.Any())
                        {
                            var refereeAssignments = new List<RefereeAssignment>();
                            var refereeIds = categoryRequest.CreateRefereeAssignmentRequests
                                .Select(r => r.RefereeAccountId)
                                .Distinct()
                                .ToList();
                            
                            var refereeAccounts = await accountRepository.GetListAsync(
                                predicate: a => refereeIds.Contains(a.Id));
                            
                            foreach (var refereeRequest in categoryRequest.CreateRefereeAssignmentRequests)
                            {
                                if (!refereeRequest.RoundTypes.Any())
                                {
                                    throw new BadRequestException("You need choose round type for referee");
                                }
                                foreach (var roundType in refereeRequest.RoundTypes)
                                {
                                    refereeAssignments.Add(new RefereeAssignment {
                                        RefereeAccountId = refereeRequest.RefereeAccountId,
                                        CompetitionCategoryId = category.Id,
                                        RoundType = roundType,
                                        AssignedAt = DateTime.UtcNow,
                                        AssignedBy = currentUserId
                                    });
                                }
                            }
                            
                            await refereeAssignmentRepository.InsertRangeAsync(refereeAssignments);
                            
                            // Lưu thông tin để gửi email sau
                            staffEmails.AddRange(refereeAccounts.Select(a => (a.Id, a.FullName ?? "NoName", a.Email ?? "NoEmail")));
                        }
                        
                        // Xử lý awards
                        if (categoryRequest.CreateAwardCateShowRequests.Any())
                        {
                            var awards = categoryRequest.CreateAwardCateShowRequests
                                .Select(a => {
                                    var award = a.Adapt<Award>();
                                    award.CompetitionCategoriesId = category.Id;
                                    return award;
                                }).ToList();
                            
                            await awardRepository.InsertRangeAsync(awards);
                        }
                    }
                }
                
                // Commit một lần
                await _unitOfWork.CommitAsync();
                await transaction.CommitAsync();
                
                // Gửi email (bất đồng bộ sau khi transaction đã commit)
                // Task.Run(() => {
                //     foreach (var staff in staffEmails)
                //     {
                //         string emailBody = ContentMailUtil.StaffRoleNotification(
                //             staff.fullName, 
                //             createShowRequest.Name, 
                //             staff.email, 
                //             "DefaultPassword123"
                //         );
                //         MailUtil.SendEmail(
                //             staff.email, 
                //             "[KOI SHOW SYSTEM] New Role Assigned", 
                //             emailBody, 
                //             null
                //         );
                //     }
                // });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var detailedMessage = $"Failed to create show and related data.\nError details: {ex.Message}";
                throw new Exception(detailedMessage);
            }
        }

        public async Task<Paginate<PaginatedKoiShowResponse>> GetPagedShowsAsync(int page, int size)
        {
            var showRepository = _unitOfWork.GetRepository<KoiShow>();
            
            var role = GetRoleFromJwt(); 
            Expression<Func<KoiShow, bool>> filterQuery = show => true;
            if (role is "Guest" or "Member")
            {
                filterQuery = filterQuery.AndAlso(show => show.Status != Domain.Enums.ShowStatus.Pending.ToString().ToLower());
            }
            else if (role is "Staff" or "Manager")
            {
                var accountId = GetIdFromJwt();
                filterQuery = filterQuery.AndAlso(show => show.ShowStaffs.Any(ss => ss.AccountId == accountId));
            }
            else if (role == "Referee")
            {
                var accountId = GetIdFromJwt();
                filterQuery = filterQuery.AndAlso(show =>
                    show.CompetitionCategories.Any(
                        c => c.RefereeAssignments.Any(ra => ra.RefereeAccountId == accountId)));
            }
            var pagedShows = await showRepository.GetPagingListAsync(
                predicate: filterQuery,
                orderBy: query => query.OrderBy(s => s.Name),
                include: query => query.Include(s => s.ShowStatuses)
                    .Include(s => s.CompetitionCategories)
                    .ThenInclude(s => s.RefereeAssignments)
                    .Include(s => s.ShowStaffs),
                page: page,
                size: size
            );

            return pagedShows.Adapt<Paginate<PaginatedKoiShowResponse>>();
        }
    }
}
