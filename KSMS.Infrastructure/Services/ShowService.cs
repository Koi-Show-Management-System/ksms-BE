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
                    .Include(x => x.TicketTypes));
            if (show is null)
            {
                throw new NotFoundException("Show is not existed");
            }
            return show.Adapt<GetKoiShowDetailResponse>();
        }

        // private async Task UpdateShowRules(KoiShow show, List<UpdateShowRuleRequestV2> newRules)
        // {
        //     var rulesRepo = _unitOfWork.GetRepository<ShowRule>();
        //
        //     var rulesToDelete = show.ShowRules.Where(r => 
        //         !newRules.Any(nr => nr.Id == r.Id)).ToList();
        //
        //     if (rulesToDelete.Any())
        //     { 
        //         rulesRepo.DeleteRangeAsync(rulesToDelete);
        //         foreach (var rule in rulesToDelete)
        //         {
        //             show.ShowRules.Remove(rule);
        //         }
        //     }
        //
        //     foreach (var ruleRequest in newRules)
        //     {
        //         var existingRule = show.ShowRules
        //             .FirstOrDefault(r => r.Id == ruleRequest.Id);
        //
        //         if (existingRule != null)
        //         {
        //             ruleRequest.Adapt(existingRule);
        //         }
        //         else
        //         {
        //             var newRule = ruleRequest.Adapt<ShowRule>();
        //             newRule.KoiShowId = show.Id;
        //             show.ShowRules.Add(newRule);
        //         }
        //     }
        // }
        private async Task UpdateShowStaffAssignments(Guid showId, ICollection<Guid> staffIds, ICollection<Guid> managerIds)
        {
            var showStaffRepo = _unitOfWork.GetRepository<ShowStaff>();
            var accountRepo = _unitOfWork.GetRepository<Account>();
            var currentUserId = GetIdFromJwt();
            
            var currentAssignments = await showStaffRepo.GetListAsync(
                predicate: x => x.KoiShowId == showId);
            
            var allNewIds = staffIds.Union(managerIds);
            var assignmentsToRemove = currentAssignments.Where(x => 
                !allNewIds.Contains(x.AccountId)).ToList();
            if (assignmentsToRemove.Any())
            { 
                showStaffRepo.DeleteRangeAsync(assignmentsToRemove);
            }
            foreach (var staffId in staffIds)
            {
                var staff = await accountRepo.SingleOrDefaultAsync(predicate: x => x.Id == staffId);
                if (staff == null)
                    throw new NotFoundException($"Staff with id {staffId} not found");
                if (!currentAssignments.Any(x => x.AccountId == staffId))
                {
                    var newAssignment = new ShowStaff
                    {
                        KoiShowId = showId,
                        AccountId = staffId,
                        AssignedBy = currentUserId,
                        AssignedAt = DateTime.UtcNow
                    };
                    await showStaffRepo.InsertAsync(newAssignment);
                }
            }
            foreach (var managerId in managerIds)
            {
                var manager = await accountRepo.SingleOrDefaultAsync(predicate: x => x.Id == managerId);
                if (manager == null)
                    throw new NotFoundException($"Manager with id {managerId} not found");

                if (!currentAssignments.Any(x => x.AccountId == managerId))
                {
                    var newAssignment = new ShowStaff
                    {
                        KoiShowId = showId,
                        AccountId = managerId,
                        AssignedBy = currentUserId,
                        AssignedAt = DateTime.UtcNow
                    };
                    await showStaffRepo.InsertAsync(newAssignment);
                }
            }
        }
        public async Task CreateShowAsync(CreateShowRequest createShowRequest)
        {
         
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

            if (string.IsNullOrWhiteSpace(createShowRequest.Name))
            {
                throw new BadRequestException("Show name cannot be empty.");
            }

            if (createShowRequest.StartDate >= createShowRequest.EndDate)
            {
                throw new BadRequestException("Start date must be earlier than end date.");
            }
            var newShow = createShowRequest.Adapt<KoiShow>();
            newShow.CreatedAt = DateTime.UtcNow;

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var createdShow = await showRepository.InsertAsync(newShow);
                await _unitOfWork.CommitAsync();
                var showId = createdShow.Id;
                if (createShowRequest.CreateShowRuleRequests.Any())
                {
                    foreach (var ruleRequest in createShowRequest.CreateShowRuleRequests)
                    {
                        var rule = ruleRequest.Adapt<ShowRule>();
                        rule.KoiShowId = showId;
                        await showRuleRepository.InsertAsync(rule);
                        await _unitOfWork.CommitAsync();
                    }
                }

                if (createShowRequest.CreateShowStatusRequests.Any())
                {
                    foreach (var statusRequest in createShowRequest.CreateShowStatusRequests)
                    {
                        var showStatus = statusRequest.Adapt<ShowStatus>();
                        showStatus.KoiShowId = showId; // Associate the show status with the show
                        await showStatusRepository.InsertAsync(showStatus);
                        await _unitOfWork.CommitAsync();
                    }
                }
                    // Process Sponsors for the Show
                if (createShowRequest.CreateSponsorRequests.Any())
                {
                    foreach (var sponsorRequest in createShowRequest.CreateSponsorRequests)
                    {
                        var sponsor = sponsorRequest.Adapt<Sponsor>();
                        sponsor.KoiShowId = showId;

                        // Save Sponsor to database
                        await sponsorRepository.InsertAsync(sponsor);
                        await _unitOfWork.CommitAsync();
                    }
                }
                // Process Ticket Types (TicketTypeRequest) associated with the show
                if (createShowRequest.CreateTicketTypeRequests.Any())
                {
                    foreach (var ticketTypeRequest in createShowRequest.CreateTicketTypeRequests)
                    {
                        var ticketType = ticketTypeRequest.Adapt<TicketType>();
                        ticketType.KoiShowId = showId;
                        await _unitOfWork.GetRepository<TicketType>().InsertAsync(ticketType);
                        await _unitOfWork.CommitAsync();
                    }
                }
                if (createShowRequest.AssignStaffRequests.Any())
                {
                    foreach (var staffId in createShowRequest.AssignStaffRequests)
                    {
                        var staff = new ShowStaff
                        {
                            KoiShowId = showId,
                            AssignedBy = GetIdFromJwt(),
                            AccountId = staffId,
                            AssignedAt = DateTime.UtcNow,
                            
                        };
                        await showStaffRepository.InsertAsync(staff);
                        await _unitOfWork.CommitAsync();
                        var staffAccount = await accountRepository.SingleOrDefaultAsync(predicate: a => a.Id == staffId);
                        if (staffAccount != null)
                        {
                            string emailBody = ContentMailUtil.StaffRoleNotification(staffAccount.FullName, createShowRequest.Name, staffAccount.Email, "DefaultPassword123");
                            MailUtil.SendEmail(staffAccount.Email, "[KOI SHOW SYSTEM] New Role Assigned", emailBody, null);
                        }
                    }
                }
                if (createShowRequest.AssignManagerRequests.Any())
                {
                    foreach (var managerId in createShowRequest.AssignManagerRequests)
                    {
                        var manager = new ShowStaff
                        {
                            KoiShowId = showId,
                            AssignedBy = GetIdFromJwt(),
                            AccountId = managerId,
                            AssignedAt = DateTime.UtcNow,
                        };
                        await showStaffRepository.InsertAsync(manager);
                        await _unitOfWork.CommitAsync();
                        var managerAccount = await accountRepository.SingleOrDefaultAsync(predicate: a => a.Id == managerId);
                        if (managerAccount != null)
                        {
                            string emailBody = ContentMailUtil.StaffRoleNotification(managerAccount.FullName, createShowRequest.Name, managerAccount.Email, "DefaultPassword123");
                            MailUtil.SendEmail(managerAccount.Email, "[KOI SHOW SYSTEM] New Role Assigned", emailBody, null);
                        }
                    }
                }
                if (createShowRequest.CreateCategorieShowRequests.Any())
                {
                    foreach (var categoryRequest in createShowRequest.CreateCategorieShowRequests)
                    {
                        var category = categoryRequest.Adapt<CompetitionCategory>();
                        category.KoiShowId = showId;
                        var createdCategory = await categoryRepository.InsertAsync(category);
                        await _unitOfWork.CommitAsync();
                        foreach (var varietyId in categoryRequest.CreateCompetionCategoryVarieties)
                        {
                            var variety = await _unitOfWork.GetRepository<Variety>()
                                .SingleOrDefaultAsync(predicate: x => x.Id == varietyId);
                            if (variety is null)
                            {
                                throw new NotFoundException("Variety with Id:" + varietyId + " not found");
                            }
                            await categoryVarietyRepository.InsertAsync(new CategoryVariety
                            {
                                CompetitionCategoryId = category.Id,
                                VarietyId = varietyId
                            });
                            await _unitOfWork.CommitAsync();
                        }
                        if (categoryRequest.CreateCriteriaCompetitionCategoryRequests.Any())
                        {
                            foreach (var groupRequest in categoryRequest.CreateCriteriaCompetitionCategoryRequests)
                            {
                                var group = groupRequest.Adapt<CriteriaCompetitionCategory>();
                                group.CompetitionCategoryId = createdCategory.Id;
                                var criterion = await criteriaRepository.SingleOrDefaultAsync( predicate: c => c.Id == groupRequest.CriteriaId);
                                if (criterion == null)
                                {
                                    throw new BadRequestException($"Criteria details are missing for CriteriaId: {groupRequest.CriteriaId}");
                                }
                                group.CriteriaId = criterion.Id;
                                await criteriaGroupRepository.InsertAsync(group);
                            }
                            await _unitOfWork.CommitAsync();
                        }
                        if (categoryRequest.CreateRoundRequests.Any())
                        {
                            foreach (var roundRequest in categoryRequest.CreateRoundRequests)
                            {
                                var round = roundRequest.Adapt<Round>();
                                round.CompetitionCategoriesId = createdCategory.Id;
                                await roundRepository.InsertAsync(round);
                            }
                            await _unitOfWork.CommitAsync();
                        }
                        if (categoryRequest.CreateRefereeAssignmentRequests.Any())
                        {
                            foreach (var refereeAssignmentRequest in categoryRequest.CreateRefereeAssignmentRequests)
                            {
                                foreach (var x in refereeAssignmentRequest.RoundTypes)
                                {
                                    var refereeAssignment = new RefereeAssignment
                                    {
                                        CompetitionCategoryId = createdCategory.Id,
                                        RoundType = x,
                                        AssignedAt = DateTime.UtcNow,
                                        AssignedBy = GetIdFromJwt()
                                    };
                                    await refereeAssignmentRepository.InsertAsync(refereeAssignment);
                                    await _unitOfWork.CommitAsync();
                                }
                                var refereeAccount = await accountRepository.SingleOrDefaultAsync(predicate: a => a.Id == refereeAssignmentRequest.RefereeAccountId);

                                var emailBody = ContentMailUtil.StaffRoleNotification(refereeAccount.FullName ?? "NoName", createShowRequest.Name, refereeAccount.Email?? "NoEmail", "DefaultPassword123");
                                MailUtil.SendEmail(refereeAccount.Email?? "NoEmail", "[KOI SHOW SYSTEM] New Referee Assigned", emailBody, null);
                            }
                        }
                        if (categoryRequest.CreateAwardCateShowRequests.Any())
                        {
                            foreach (var awardRequest in categoryRequest.CreateAwardCateShowRequests)
                            {
                                var award = awardRequest.Adapt<Award>();
                                award.CompetitionCategoriesId = createdCategory.Id;
                                await awardRepository.InsertAsync(award);
                                await _unitOfWork.CommitAsync();
                            }
                        }
                    }
                }
                await _unitOfWork.CommitAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Failed to create show and related data.", ex);
            }
        }
        

        //public async Task<KoiShowResponse> GetShowByIdAsync(Guid id)
        //{
        //    var showRepository = _unitOfWork.GetRepository<KoiShow>();
                
        //    var show = await showRepository.SingleOrDefaultAsync(
        //         predicate: s => s.Id == id,
        //        orderBy: q => q.OrderBy(s => s.Name),
        //        include: query => query.Include(s => s.ShowStatuses)
        //        .Include(s => s.CompetitionCategories)
        //            .Include(s => s.CompetitionCategories)
        //                .ThenInclude(s => s.Rounds)
        //            .Include(s => s.CompetitionCategories)
        //                .ThenInclude(s => s.Awards)
        //            .Include(s => s.CompetitionCategories)
        //                .ThenInclude(s => s.CriteriaCompetitionCategories)
        //                    .ThenInclude(s => s.Criteria)
        //            .Include(s => s.CompetitionCategories)
        //                .ThenInclude(s => s.RefereeAssignments)
        //                    .ThenInclude(s => s.RefereeAccount)
        //            .Include(s => s.CompetitionCategories)
        //                .ThenInclude(s => s.Registrations)
        //             .Include(s => s.CompetitionCategories)
        //                .ThenInclude(s => s.CategoryVarieties)
        //                    .ThenInclude(s => s.Variety) 
        //            .Include(s => s.CompetitionCategories)
        //                .ThenInclude(s => s.RefereeAssignments)
        //                    .ThenInclude(s => s.AssignedByNavigation)

        //            .Include(s => s.ShowStaffs)
        //                .ThenInclude(s => s.AssignedByNavigation)

        //            .Include(s => s.ShowStaffs)
        //                .ThenInclude(s => s.Account)

        //            .Include(s => s.ShowRules)
        //            .Include(s => s.Sponsors)
        //            .Include(s => s.TicketTypes)
        //            .Include(s => s.CompetitionCategories)
        //                .ThenInclude(s => s.CriteriaCompetitionCategories)
        //                    .ThenInclude(c => c.Criteria)
        //                        .ThenInclude(e => e.ErrorTypes)


        //    );

        //    if (show == null)
        //    {
        //        throw new NotFoundException("Show not found.");
        //    }

        //    return show.Adapt<KoiShowResponse>();
        //}


        public async Task UpdateShowAsync(Guid id, UpdateShowRequest updateShowRequest)
        {
 

            var showStatusRepository = _unitOfWork.GetRepository<ShowStatus>();
            var showRepository = _unitOfWork.GetRepository<KoiShow>();
            var categoryRepository = _unitOfWork.GetRepository<CompetitionCategory>();
            var roundRepository = _unitOfWork.GetRepository<Round>();
            var criteriaGroupRepository = _unitOfWork.GetRepository<CriteriaCompetitionCategory>();
            var criteriaRepository = _unitOfWork.GetRepository<Criterion>();
            var refereeAssignmentRepository = _unitOfWork.GetRepository<RefereeAssignment>();
            var showStaffRepository = _unitOfWork.GetRepository<ShowStaff>();
            var showRuleRepository = _unitOfWork.GetRepository<ShowRule>();
            var sponsorRepository = _unitOfWork.GetRepository<Sponsor>();
            var ticketRepository = _unitOfWork.GetRepository<TicketType>();
            var awardRepository = _unitOfWork.GetRepository<Award>();
            var varietyRepository = _unitOfWork.GetRepository<Variety>();
            var categoryVarietyRepository = _unitOfWork.GetRepository<CategoryVariety>();

            // Fetch the show from the repository
            var show = await showRepository.SingleOrDefaultAsync(predicate: s => s.Id == id);
            if (show == null)
            {
                throw new NotFoundException("Show not found.");
            }

            // Update properties of Show
            show.StartDate = updateShowRequest.StartDate ?? show.StartDate;
            show.EndDate = updateShowRequest.EndDate ?? show.EndDate;
            show.StartExhibitionDate = updateShowRequest.StartExhibitionDate ?? show.StartExhibitionDate;
            show.EndExhibitionDate = updateShowRequest.EndExhibitionDate ?? show.EndExhibitionDate;
            show.Location = updateShowRequest.Location ?? show.Location;
            show.Description = updateShowRequest.Description ?? show.Description;
            show.RegistrationDeadline = updateShowRequest.RegistrationDeadline ?? show.RegistrationDeadline;
            show.MinParticipants = updateShowRequest.MinParticipants ?? show.MinParticipants;
            show.MaxParticipants = updateShowRequest.MaxParticipants ?? show.MaxParticipants;
            show.HasGrandChampion = updateShowRequest.HasGrandChampion;
            show.HasBestInShow = updateShowRequest.HasBestInShow;
            show.ImgUrl = updateShowRequest.ImgUrl ?? show.ImgUrl;
            show.Name = updateShowRequest.Name ?? show.Name;
            show.Status = updateShowRequest.Status ?? show.Status;
            show.UpdatedAt = DateTime.Now;

            // Update Show
            showRepository.UpdateAsync(show);

            // Update Categories and related entities
            if (updateShowRequest.UpdateCategorieShowRequests != null && updateShowRequest.UpdateCategorieShowRequests.Any())
            {
                foreach (var categoryRequest in updateShowRequest.UpdateCategorieShowRequests)
                {
                    var category = await categoryRepository.SingleOrDefaultAsync( predicate: c => c.Id == categoryRequest.Id && c.KoiShowId == id);
                    if (category != null)
                    {
                        category.Name = categoryRequest.Name;
                        category.SizeMin = categoryRequest.SizeMin;
                        category.SizeMax = categoryRequest.SizeMax;
                        category.Description = categoryRequest.Description;
                        category.MaxEntries = categoryRequest.MaxEntries;
                        category.StartTime = categoryRequest.StartTime ?? category.StartTime;
                        category.EndTime = categoryRequest.EndTime ?? category.EndTime;
                        category.Status = categoryRequest.Status;

                        // Update Category
                        categoryRepository.UpdateAsync(category);

                        // Update Variety
                        if (categoryRequest.UpdateCategoryVarietyRequests != null && categoryRequest.UpdateCategoryVarietyRequests.Any())
                        {
                            foreach (var categoryVarietyRequest in categoryRequest.UpdateCategoryVarietyRequests)
                            {
                                // Check if the variety exists
                                var variety = await varietyRepository.SingleOrDefaultAsync(predicate: v => v.Id == categoryVarietyRequest.VarietyId);

                                // If variety doesn't exist, create a new one
                                if (variety == null)
                                {
                                    throw new NotFoundException("Variety not found.");
                                }
                                 

                                // Now check if CategoryVariety with the given CategoryId and VarietyId exists
                                var existingCategoryVariety = await categoryVarietyRepository.SingleOrDefaultAsync(predicate: cv => cv.CompetitionCategoryId == categoryRequest.Id && cv.VarietyId == variety.Id);

                                if (existingCategoryVariety != null)
                                {
                                    // If CategoryVariety exists but the VarietyId is different, update the VarietyId
                                    if (existingCategoryVariety.VarietyId != variety.Id)
                                    {
                                        existingCategoryVariety.VarietyId = variety.Id;

                                        // Update the CategoryVariety with the new VarietyId
                                        categoryVarietyRepository.UpdateAsync(existingCategoryVariety);
                                    }
                                }
                            }
                        }

                        // Update Awards
                        if (categoryRequest.UpdateAwardCateShowRequests != null && categoryRequest.UpdateAwardCateShowRequests.Any())
                        {
                            foreach (var awardRequest in categoryRequest.UpdateAwardCateShowRequests)
                            {
                                var award = await awardRepository.SingleOrDefaultAsync(predicate: a => a.CompetitionCategoriesId == category.Id && a.Id == awardRequest.Id);
                                if (award != null)
                                {
                                    award.Name = awardRequest.Name;
                                    award.AwardType = awardRequest.AwardType;
                                    award.PrizeValue = awardRequest.PrizeValue;
                                    award.Description = awardRequest.Description;

                                    awardRepository.UpdateAsync(award);
                                }
                                else
                                {
                                    var newAward = new Award
                                    {
                                        CompetitionCategoriesId = category.Id,
                                        Name = awardRequest.Name,
                                        AwardType = awardRequest.AwardType,
                                        PrizeValue = awardRequest.PrizeValue,
                                        Description = awardRequest.Description
                                    };
                                    await awardRepository.InsertAsync(newAward);
                                }
                            }
                        }

                        // Update CriteriaGroups and Criteria
                        if (categoryRequest.UpdateCriteriaCompetitionCategoryRequests != null && categoryRequest.UpdateCriteriaCompetitionCategoryRequests.Any())
                        {
                            foreach (var criteriaGroupRequest in categoryRequest.UpdateCriteriaCompetitionCategoryRequests)
                            {
                                var criteriaGroup = await criteriaGroupRepository.SingleOrDefaultAsync(predicate: g => g.CompetitionCategoryId == category.Id && g.Id == criteriaGroupRequest.Id);
                                if (criteriaGroup != null)
                                {
                                    criteriaGroup.RoundType      = criteriaGroupRequest.RoundType;
                                    criteriaGroup.CriteriaId = criteriaGroupRequest.CriteriaId;
                                    criteriaGroup.RoundType = criteriaGroupRequest.RoundType;
                                    criteriaGroup.Order = criteriaGroupRequest.Order;
                                    criteriaGroupRepository.UpdateAsync(criteriaGroup);

                                    var criterion = await criteriaRepository.SingleOrDefaultAsync(predicate: c => c.Id == criteriaGroupRequest.CriteriaId);
                                    if (criterion == null)
                                    {
                                        throw new BadRequestException(" criterion not found");
                                    }

                                    criteriaGroup.CriteriaId = criterion.Id;
                                    criteriaGroupRepository.UpdateAsync(criteriaGroup);
                                }
                            }
                        }

                        // Update Rounds
                        if (categoryRequest.UpdateRoundRequest != null && categoryRequest.UpdateRoundRequest.Any())
                        {
                            foreach (var roundRequest in categoryRequest.UpdateRoundRequest)
                            {
                                var round = await roundRepository.SingleOrDefaultAsync(predicate: r => r.CompetitionCategoriesId == category.Id && r.Id == roundRequest.Id);
                                if (round != null)
                                {
                                    round.Name = roundRequest.Name;
                                    round.RoundOrder = roundRequest.RoundOrder;
                                    round.RoundType = roundRequest.RoundType;
                                    round.StartTime = roundRequest.StartTime ?? round.StartTime;
                                    round.EndTime = roundRequest.EndTime ?? round.EndTime;
                                    round.MinScoreToAdvance = roundRequest.MinScoreToAdvance;
                                    round.Status = roundRequest.Status;

                                    roundRepository.UpdateAsync(round);
                                }
                                else
                                {
                                    var newRound = new Round
                                    {
                                        CompetitionCategoriesId = category.Id,
                                        Name = roundRequest.Name,
                                        RoundOrder = roundRequest.RoundOrder,
                                        RoundType = roundRequest.RoundType,
                                        StartTime = roundRequest.StartTime ?? round.StartTime,
                                        EndTime = roundRequest.EndTime ?? round.EndTime,
                                        MinScoreToAdvance = roundRequest.MinScoreToAdvance,
                                        Status = roundRequest.Status
                                    };
                                    await roundRepository.InsertAsync(newRound);
                                }
                            }
                        }

                        // Update RefereeAssignments
                        if (categoryRequest.UpdateRefereeAssignmentRequests != null && categoryRequest.UpdateRefereeAssignmentRequests.Any())
                        {
                            foreach (var refereeRequest in categoryRequest.UpdateRefereeAssignmentRequests)
                            {
                                var refereeAssignment = new RefereeAssignment
                                {
                                    CompetitionCategoryId = category.Id,
                                    RefereeAccountId = refereeRequest.RefereeAccountId,
                                    AssignedAt = refereeRequest.AssignedAt ?? DateTime.Now,
                                    RoundType = refereeRequest.RoundType,
                                    AssignedBy = refereeRequest.AssignedBy
                                };
                                refereeAssignmentRepository.UpdateAsync(refereeAssignment);
                            }
                        }
                    }
                }
            }
            // Update ShowStaffs
            if (updateShowRequest.UpdateShowStaffRequests != null && updateShowRequest.UpdateShowStaffRequests.Any())
            {
                foreach (var staffRequest in updateShowRequest.UpdateShowStaffRequests)
                {
                    var staff = await showStaffRepository.SingleOrDefaultAsync(predicate: s => s.Id == staffRequest.Id && s.KoiShowId == id);

                    if (staff != null)
                    {
                        // Update existing ShowStaff
                        staff.AccountId = staffRequest.AccountId;
                        staff.AssignedBy = staffRequest.AssignedBy;
                        staff.AssignedAt = staffRequest.AssignedAt ?? staff.AssignedAt;

                        // Update ShowStaff in the database
                        showStaffRepository.UpdateAsync(staff);
                    }
                    else
                    {
                        // If ShowStaff not found, create a new ShowStaff
                        var newStaff = new ShowStaff
                        {
                            KoiShowId = id,
                            AccountId = staffRequest.AccountId,
                            AssignedBy = staffRequest.AssignedBy,
                            AssignedAt = staffRequest.AssignedAt ?? DateTime.Now
                        };

                        // Insert new ShowStaff into the database
                        await showStaffRepository.InsertAsync(newStaff);
                    }
                }
            }

            // Update TicketTypes
            if (updateShowRequest.UpdateTicketRequests != null && updateShowRequest.UpdateTicketRequests.Any())
            {
                foreach (var ticketRequest in updateShowRequest.UpdateTicketRequests)
                {
                    var ticket = await ticketRepository.SingleOrDefaultAsync(predicate: t => t.Id == ticketRequest.Id && t.KoiShowId == id);
                    if (ticket != null)
                    {
                        ticket.Name = ticketRequest.Name;
                        ticket.Price = ticketRequest.Price;
                        ticket.AvailableQuantity = ticketRequest.AvailableQuantity;
                        ticketRepository.UpdateAsync(ticket);
                    }
                    else
                    {
                        var newTicket = new TicketType
                        {
                            KoiShowId = id,
                            Name = ticketRequest.Name,
                            Price = ticketRequest.Price,
                            AvailableQuantity = ticketRequest.AvailableQuantity
                        };
                        await ticketRepository.InsertAsync(newTicket);
                    }
                }
            }

            // Update Sponsors
            if (updateShowRequest.UpdateSponsorRequests != null && updateShowRequest.UpdateSponsorRequests.Any())
            {
                foreach (var sponsorRequest in updateShowRequest.UpdateSponsorRequests)
                {
                    var sponsor = await sponsorRepository.SingleOrDefaultAsync(predicate: s => s.Id == sponsorRequest.Id && s.KoiShowId == id);
                    if (sponsor != null)
                    {
                        sponsor.Name = sponsorRequest.Name;
                        sponsor.LogoUrl = sponsorRequest.LogoUrl;
                        sponsorRepository.UpdateAsync(sponsor);
                    }
                    else
                    {
                        var newSponsor = new Sponsor
                        {
                            KoiShowId = id,
                            Name = sponsorRequest.Name,
                            LogoUrl = sponsorRequest.LogoUrl
                        };
                        await sponsorRepository.InsertAsync(newSponsor);
                    }
                }
            }

            // Update ShowRules
            if (updateShowRequest.UpdateShowRuleRequests != null && updateShowRequest.UpdateShowRuleRequests.Any())
            {
                foreach (var ruleRequest in updateShowRequest.UpdateShowRuleRequests)
                {
                    var rule = await showRuleRepository.SingleOrDefaultAsync(predicate: r => r.Id == ruleRequest.Id && r.KoiShowId == id);
                    if (rule != null)
                    {
                        rule.Title = ruleRequest.Title;
                        rule.Content = ruleRequest.Content;
                        showRuleRepository.UpdateAsync(rule);
                    }
                    else
                    {
                        var newRule = new ShowRule
                        {
                            KoiShowId = id,
                            Title = ruleRequest.Title,
                            Content = ruleRequest.Content
                        };
                        await showRuleRepository.InsertAsync(newRule);
                    }
                }
            }

            // Update ShowStatuses
            if (updateShowRequest.UpdateShowStatusRequests != null && updateShowRequest.UpdateShowStatusRequests.Any())
            {
                foreach (var statusRequest in updateShowRequest.UpdateShowStatusRequests)
                {
                    var status = await showStatusRepository.SingleOrDefaultAsync(predicate: s => s.Id == statusRequest.Id && s.KoiShowId == id);
                    if (status != null)
                    {
                        status.StatusName = statusRequest.StatusName;
                        status.Description = statusRequest.Description;
                        status.StartDate = statusRequest.StartDate;
                        status.EndDate = statusRequest.EndDate;
                        status.IsActive = statusRequest.IsActive;
                        showStatusRepository.UpdateAsync(status);
                    }
                    else
                    {
                        var newStatus = new ShowStatus
                        {
                            KoiShowId = id,
                            StatusName = statusRequest.StatusName,
                            Description = statusRequest.Description,
                            StartDate = statusRequest.StartDate,
                            EndDate = statusRequest.EndDate,
                            IsActive = statusRequest.IsActive
                        };
                        await showStatusRepository.InsertAsync(newStatus);
                    }
                }
            }

            try
            {
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                // Lấy inner exception và ghi log hoặc ném lại thông báo lỗi chi tiết
                Console.WriteLine(ex.InnerException?.Message ?? ex.Message);
                throw;
            }

        }

        public async Task<Paginate<PaginatedKoiShowResponse>> GetPagedShowsAsync(int page, int size)
        {
            var showRepository = _unitOfWork.GetRepository<KoiShow>();
            
            var role = GetRoleFromJwt(); 
            Expression<Func<KoiShow, bool>> filterQuery = show => true;
            if (role is "Guest" or "Member")
            {
                filterQuery = filterQuery.AndAlso(show => show.Status != Domain.Enums.ShowStatus.Draft.ToString().ToLower());
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
        /// <summary>
        /// Cập nhật trạng thái của ShowStatus theo thời gian
        /// </summary>
        private async Task UpdateShowStatusAsync(Guid showId)
        {
            var showStatusRepository = _unitOfWork.GetRepository<ShowStatus>();

            var currentTime = DateTime.UtcNow;

            // Lấy danh sách các ShowStatus liên quan đến showId
            var showStatuses = await showStatusRepository.GetListAsync(
                predicate: s => s.KoiShowId == showId,
                orderBy: q => q.OrderBy(s => s.StartDate) // Đảm bảo xử lý theo thứ tự thời gian
            );

            if (showStatuses.Any())
            {
                bool hasChanges = false;

                foreach (var status in showStatuses)
                {
                    if (status.StartDate <= currentTime && status.EndDate > currentTime)
                    {
                        status.StatusName = "Ongoing"; // Đang diễn ra
                        status.IsActive = true;
                        hasChanges = true;
                    }
                    else if (status.EndDate <= currentTime)
                    {
                        status.StatusName = "Completed"; // Đã kết thúc
                        status.IsActive = false;
                        hasChanges = true;
                    }
                }

                // Nếu có thay đổi thì update vào database
                if (hasChanges)
                {
                    showStatusRepository.UpdateRange(showStatuses);
                    await _unitOfWork.CommitAsync();
                }
            }
        }


    }
}
