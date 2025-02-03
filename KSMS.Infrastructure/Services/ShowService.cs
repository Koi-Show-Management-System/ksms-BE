using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.Show;
using KSMS.Domain.Dtos.Responses.Show;
using KSMS.Domain.Dtos.Responses.ShowStatus;
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
using static KSMS.Infrastructure.Utils.MailUtil;

namespace KSMS.Infrastructure.Services
{
    public class ShowService : BaseService<ShowService>, IShowService
    {
        public ShowService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<ShowService> logger) : base(unitOfWork, logger)
        {
        }

        public async Task<ShowResponse> CreateShowAsync(CreateShowRequest createShowRequest)
        {
            // Lấy các repository cần thiết
            var showRepository = _unitOfWork.GetRepository<Show>();
            var categoryRepository = _unitOfWork.GetRepository<Category>();
            var varietyRepository = _unitOfWork.GetRepository<Variety>();
            var sponsorRepository = _unitOfWork.GetRepository<Sponsor>();
            var showStaffRepository = _unitOfWork.GetRepository<ShowStaff>();
            var showRuleRepository = _unitOfWork.GetRepository<ShowRule>();
            var showStatusRepository = _unitOfWork.GetRepository<ShowStatus>();
            var showStatisticRepository = _unitOfWork.GetRepository<ShowStatistic>();
            var ticketRepository = _unitOfWork.GetRepository<Ticket>();
            var roundRepository = _unitOfWork.GetRepository<Round>();
            var criteriaGroupRepository = _unitOfWork.GetRepository<CriteriaGroup>();
            var criteriaRepository = _unitOfWork.GetRepository<Criterion>();
            var refereeAssignmentRepository = _unitOfWork.GetRepository<RefereeAssignment>();
            var accountRepository = _unitOfWork.GetRepository<Account>();
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(createShowRequest.Name))
            {
                throw new BadRequestException("Show name cannot be empty.");
            }

            if (createShowRequest.StartDate >= createShowRequest.EndDate)
            {
                throw new BadRequestException("Start date must be earlier than end date.");
            }

            // Tạo thực thể Show
            var newShow = createShowRequest.Adapt<Show>();
            newShow.CreatedAt = DateTime.UtcNow;

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Lưu Show
                var createdShow = await showRepository.InsertAsync(newShow);
                await _unitOfWork.CommitAsync();
                var showId = createdShow.Id;

                // Xử lý Categories
                if (createShowRequest.Categories != null && createShowRequest.Categories.Any())
                {
                    foreach (var categoryRequest in createShowRequest.Categories)
                    {
                        Guid? varietyId = null;

                        // Xử lý Variety trước
                        if (categoryRequest.Variety != null)
                        {
                            var newVariety = categoryRequest.Variety.Adapt<Variety>();
                            var createdVariety = await varietyRepository.InsertAsync(newVariety);
                            await _unitOfWork.CommitAsync();
                            varietyId = createdVariety.Id;
                        }

                        // Tạo Category
                        var category = categoryRequest.Adapt<Category>();
                        category.ShowId = showId;
                        category.VarietyId = varietyId;
                        var createdCategory = await categoryRepository.InsertAsync(category);
                        await _unitOfWork.CommitAsync();

                        // Xử lý Rounds cho Category
                        if (categoryRequest.Rounds != null && categoryRequest.Rounds.Any())
                        {
                            foreach (var roundRequest in categoryRequest.Rounds)
                            {
                                var round = roundRequest.Adapt<Round>();
                                round.CategoryId = createdCategory.Id;
                                await roundRepository.InsertAsync(round);
                            }
                        }

                        // Xử lý CriteriaGroups và Criterias cho Category
                        if (categoryRequest.CriteriaGroups != null && categoryRequest.CriteriaGroups.Any())
                        {
                            foreach (var groupRequest in categoryRequest.CriteriaGroups)
                            {
                                var group = groupRequest.Adapt<CriteriaGroup>();
                                group.CategoryId = createdCategory.Id;
                                var createdGroup = await criteriaGroupRepository.InsertAsync(group);
                                await _unitOfWork.CommitAsync();

                                if (groupRequest.Criterias != null && groupRequest.Criterias.Any())
                                {
                                    foreach (var criteriaRequest in groupRequest.Criterias)
                                    {
                                        var criteria = criteriaRequest.Adapt<Criterion>();
                                        criteria.CriteriaGroupId = createdGroup.Id;
                                        await criteriaRepository.InsertAsync(criteria);
                                    }
                                }
                            }
                        }

                        // Xử lý RefereeAssignments cho Category
                        if (categoryRequest.RefereeAssignments != null && categoryRequest.RefereeAssignments.Any())
                        {
                            foreach (var refereeRequest in categoryRequest.RefereeAssignments)
                            {
                                var refereeAssignment = refereeRequest.Adapt<RefereeAssignment>();
                                refereeAssignment.CategoryId = createdCategory.Id;
                                await refereeAssignmentRepository.InsertAsync(refereeAssignment);
                                var refereeAccount = await accountRepository.SingleOrDefaultAsync(a => a.Id == refereeAssignment.RefereeAccountId, null, null);
                                if (refereeAccount != null)
                                {
                                    // Prepare email content with login credentials
                                    var username = refereeAccount.Username; // Assuming Username is stored in the Account entity
                                    
                                    var subject = "[KOI SHOW SYSTEM] New Role Assigned";
                                    var body = ContentMailUtil.StaffRoleNotification(refereeAccount.FullName, createdShow.Name, username, refereeAccount.HashedPassword);

                                    // Send email to referee
                                    bool emailSent = MailUtil.SendEmail(refereeAccount.Email, subject, body, null);
                                    if (!emailSent)
                                    {
                                        throw new Exception($"Failed to send email to referee {refereeAccount.FullName}");
                                    }
                                }
                            }
                        }
                    }
                }

                // Xử lý Sponsors
                if (createShowRequest.Sponsors != null && createShowRequest.Sponsors.Any())
                {
                    foreach (var sponsorRequest in createShowRequest.Sponsors)
                    {
                        var sponsor = sponsorRequest.Adapt<Sponsor>();
                        sponsor.ShowId = showId;
                        await sponsorRepository.InsertAsync(sponsor);
                    }
                }

                // Xử lý Show Staffs
                //if (createShowRequest.ShowStaffs != null && createShowRequest.ShowStaffs.Any())
                //{
                //    foreach (var staffRequest in createShowRequest.ShowStaffs)
                //    {
                //        var showStaff = staffRequest.Adapt<ShowStaff>();
                //        showStaff.ShowId = showId;
                //        await showStaffRepository.InsertAsync(showStaff);
                //    }
                //}
                if (createShowRequest.ShowStaffs != null && createShowRequest.ShowStaffs.Any())
                {
                    foreach (var staffRequest in createShowRequest.ShowStaffs)
                    {
                        var showStaff = staffRequest.Adapt<ShowStaff>();
                        showStaff.ShowId = showId;
                        var createdStaff = await showStaffRepository.InsertAsync(showStaff);

                        // Fetch the staff's email and full name
                        var staffAccount = await accountRepository.SingleOrDefaultAsync(a => a.Id == showStaff.AccountId,null,null);
                        if (staffAccount != null)
                        {
                            var subject = "[KOI SHOW SYSTEM] New Show Management Role Assigned";
                            var body = ContentMailUtil.StaffRoleNotification(staffAccount.FullName, createdShow.Name, staffAccount.Email, staffAccount.HashedPassword);

                            // Send email
                            bool emailSent = MailUtil.SendEmail(staffAccount.Email, subject, body, null);
                            if (!emailSent)
                            {
                                throw new Exception($"Failed to send email to staff {staffAccount.FullName}");
                            }
                        }
                    }
                }
                // Xử lý Show Rules
                if (createShowRequest.ShowRules != null && createShowRequest.ShowRules.Any())
                {
                    foreach (var ruleRequest in createShowRequest.ShowRules)
                    {
                        var showRule = ruleRequest.Adapt<ShowRule>();
                        showRule.ShowId = showId;
                        await showRuleRepository.InsertAsync(showRule);
                    }
                }

                // Xử lý Show Statuses
                if (createShowRequest.ShowStatuses != null && createShowRequest.ShowStatuses.Any())
                {
                    foreach (var statusRequest in createShowRequest.ShowStatuses)
                    {
                        var showStatus = statusRequest.Adapt<ShowStatus>();
                        showStatus.ShowId = showId;
                        await showStatusRepository.InsertAsync(showStatus);
                    }
                }

                // Xử lý Show Statistics
                if (createShowRequest.ShowStatistics != null && createShowRequest.ShowStatistics.Any())
                {
                    foreach (var statisticRequest in createShowRequest.ShowStatistics)
                    {
                        var showStatistic = statisticRequest.Adapt<ShowStatistic>();
                        showStatistic.ShowId = showId;
                        await showStatisticRepository.InsertAsync(showStatistic);
                    }
                }

                // Xử lý Tickets
                if (createShowRequest.Tickets != null && createShowRequest.Tickets.Any())
                {
                    foreach (var ticketRequest in createShowRequest.Tickets)
                    {
                        var ticket = ticketRequest.Adapt<Ticket>();
                        ticket.ShowId = showId;
                        await ticketRepository.InsertAsync(ticket);
                    }
                }

                // Commit toàn bộ giao dịch
                await _unitOfWork.CommitAsync();
                await transaction.CommitAsync();

                // Trả về kết quả
                return createdShow.Adapt<ShowResponse>();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Failed to create show and related data.", ex);
            }
        }

        public async Task<IEnumerable<ShowResponse>> GetAllShowsAsync()
        {
         
            var showRepository = _unitOfWork.GetRepository<Show>();

             
            var shows = await showRepository.GetListAsync(
                predicate: null,  
                orderBy: q => q.OrderBy(s => s.Name),
             include: query => query.Include(s => s.ShowStatuses)
                    .Include(s => s.Categories)
                        .ThenInclude(s => s.Rounds)
                    .Include(s => s.Categories)
                        .ThenInclude(s => s.Awards)
                    .Include(s => s.Categories)
                        .ThenInclude(s => s.CriteriaGroups)
                            .ThenInclude(s => s.Criteria)
                    .Include(s => s.Categories)
                        .ThenInclude(s => s.RefereeAssignments)
                            .ThenInclude(s => s.RefereeAccount)
                                .ThenInclude(s => s.Role)
                    .Include(s => s.Categories)
                        .ThenInclude(s => s.RefereeAssignments)
                            .ThenInclude(s => s.AssignedByNavigation)
                                .ThenInclude(s => s.Role)
                    .Include(s => s.ShowStaffs)
                        .ThenInclude(s => s.AssignedByNavigation)
                            .ThenInclude(s => s.Role)
                    .Include(s => s.ShowStaffs)
                        .ThenInclude(s => s.Account)
                            .ThenInclude(s => s.Role)
                    .Include(s => s.ShowRules)
                    .Include(s => s.ShowStatistics)
                    .Include(s => s.Sponsors)
                    .Include(s => s.Tickets)
            );

          
            var showResponses = shows.Select(show => show.Adapt<ShowResponse>());

            
            return showResponses;
        }

        public async Task<ShowResponse> GetShowByIdAsync(Guid id)
        {
            //await UpdateShowStatusAsync(id); // Cập nhật trạng thái trước khi trả về Show

            var showRepository = _unitOfWork.GetRepository<Show>();

            var show = await showRepository.SingleOrDefaultAsync(
                predicate: s => s.Id == id,
                include: query => query.Include(s => s.ShowStatuses)
                    .Include(s => s.Categories)
                        .ThenInclude(s => s.Rounds)
                    .Include(s => s.Categories)
                        .ThenInclude(s => s.Awards)
                    .Include(s => s.Categories)
                        .ThenInclude(s => s.CriteriaGroups)
                            .ThenInclude(s => s.Criteria)
                    .Include(s => s.Categories)
                        .ThenInclude(s => s.RefereeAssignments)
                            .ThenInclude(s => s.RefereeAccount)
                                .ThenInclude(s => s.Role)
                     .Include(s => s.Categories)
                        .ThenInclude(s => s.Variety)
                    .Include(s => s.Categories)
                        .ThenInclude(s => s.RefereeAssignments)
                            .ThenInclude(s => s.AssignedByNavigation)
                                .ThenInclude(s => s.Role)
                    .Include(s => s.ShowStaffs)
                        .ThenInclude(s => s.AssignedByNavigation)
                            .ThenInclude(s => s.Role)
                    .Include(s => s.ShowStaffs)
                        .ThenInclude(s => s.Account)
                            .ThenInclude(s => s.Role)
                    .Include(s => s.ShowRules)
                    .Include(s => s.ShowStatistics)
                    .Include(s => s.Sponsors)
                    .Include(s => s.Tickets)
            );

            if (show == null)
            {
                throw new NotFoundException("Show not found.");
            }

            return show.Adapt<ShowResponse>();
        }

        public Task PatchShowStatusAsync(Guid id, string statusName)
        {
            throw new NotImplementedException();
        }
        public async Task UpdateShowAsync(Guid id, UpdateShowRequest updateShowRequest)
        {
            var showRepository = _unitOfWork.GetRepository<Show>();
            var categoryRepository = _unitOfWork.GetRepository<Category>();
            var roundRepository = _unitOfWork.GetRepository<Round>();
            var criteriaGroupRepository = _unitOfWork.GetRepository<CriteriaGroup>();
            var criteriaRepository = _unitOfWork.GetRepository<Criterion>();
            var refereeAssignmentRepository = _unitOfWork.GetRepository<RefereeAssignment>();
            var showStaffRepository = _unitOfWork.GetRepository<ShowStaff>();
            var showRuleRepository = _unitOfWork.GetRepository<ShowRule>();
            var showStatisticRepository = _unitOfWork.GetRepository<ShowStatistic>();
            var showStatusRepository = _unitOfWork.GetRepository<ShowStatus>();
            var sponsorRepository = _unitOfWork.GetRepository<Sponsor>();
            var ticketRepository = _unitOfWork.GetRepository<Ticket>();
            var awardRepository = _unitOfWork.GetRepository<Award>();
            var varietyRepository = _unitOfWork.GetRepository<Variety>();

            // Fetch the show from the repository
            var show = await showRepository.SingleOrDefaultAsync(
                predicate: s => s.Id == id,
                include: null // Include related entities if needed
            );

            if (show == null)
            {
                throw new NotFoundException("Show not found.");
            }

            // Validation for DateTime
            DateTime? ValidateDate(DateTime? date)
            {
                if (date.HasValue && date.Value >= new DateTime(1753, 1, 1) && date.Value <= new DateTime(9999, 12, 31))
                {
                    return date;
                }
                return null;
            }

            // Update properties of Show
            show.StartDate = ValidateDate(updateShowRequest.StartDate) ?? show.StartDate;
            show.EndDate = ValidateDate(updateShowRequest.EndDate) ?? show.EndDate;
            show.StartExhibitionDate = ValidateDate(updateShowRequest.StartExhibitionDate) ?? show.StartExhibitionDate;
            show.EndExhibitionDate = ValidateDate(updateShowRequest.EndExhibitionDate) ?? show.EndExhibitionDate;
            show.Location = updateShowRequest.Location;
            show.Description = updateShowRequest.Description;
            show.RegistrationDeadline = updateShowRequest.RegistrationDeadline;
            show.MinParticipants = updateShowRequest.MinParticipants;
            show.MaxParticipants = updateShowRequest.MaxParticipants;
            show.HasGrandChampion = updateShowRequest.HasGrandChampion;
            show.HasBestInShow = updateShowRequest.HasBestInShow;
            show.ImgUrl = updateShowRequest.ImgUrl;
            show.RegistrationFee = updateShowRequest.RegistrationFee;
            show.Name = updateShowRequest.Name;
            show.Status = updateShowRequest.Status;
            show.UpdatedAt = DateTime.Now;

            // Update Show
             showRepository.UpdateAsync(show);

            // Update Categories and related entities
            if (updateShowRequest.Categories != null && updateShowRequest.Categories.Any())
            {
                foreach (var categoryRequest in updateShowRequest.Categories)
                {
                    var category = await categoryRepository.SingleOrDefaultAsync(c => c.Id == categoryRequest.Id && c.ShowId == id, null, null);

                    if (category != null)
                    {
                        // Detach entity to avoid tracking errors
                        _unitOfWork.Detach(category);

                        category.Name = categoryRequest.Name;
                        category.SizeMin = categoryRequest.SizeMin;
                        category.SizeMax = categoryRequest.SizeMax;
                        category.VarietyId = categoryRequest.VarietyId;
                        category.Description = categoryRequest.Description;
                        category.MaxEntries = categoryRequest.MaxEntries;
                        category.StartTime = categoryRequest.StartTime ?? category.StartTime;
                        category.EndTime = categoryRequest.EndTime ?? category.EndTime;
                        category.Status = categoryRequest.Status;

                        // Update Category
                         categoryRepository.UpdateAsync(category);

                        // Update Variety
                        if (categoryRequest.Variety != null)
                        {
                            var variety = await varietyRepository.SingleOrDefaultAsync(v => v.Id == categoryRequest.VarietyId, null, null);
                            if (variety != null)
                            {
                                variety.Name = categoryRequest.Variety.Name;
                                variety.Description = categoryRequest.Variety.Description;
                                 varietyRepository.UpdateAsync(variety);
                            }
                            else
                            {
                                var newVariety = new Variety
                                {
                                    Name = categoryRequest.Variety.Name,
                                    Description = categoryRequest.Variety.Description
                                };
                                await varietyRepository.InsertAsync(newVariety);
                            }
                        }

                        // Update Rounds
                        if (categoryRequest.Rounds != null && categoryRequest.Rounds.Any())
                        {
                            foreach (var roundRequest in categoryRequest.Rounds)
                            {
                                var round = await roundRepository.SingleOrDefaultAsync(r => r.CategoryId == category.Id && r.Id == roundRequest.Id, null, null);
                                if (round != null)
                                {
                                    round.Name = roundRequest.Name;
                                    round.RoundOrder = roundRequest.RoundOrder;
                                    round.RoundType = roundRequest.RoundType;
                                    round.StartTime = ValidateDate(roundRequest.StartTime) ?? round.StartTime;
                                    round.EndTime = ValidateDate(roundRequest.EndTime) ?? round.EndTime;
                                    round.MinScoreToAdvance = roundRequest.MinScoreToAdvance;
                                    round.Status = roundRequest.Status;

                                     roundRepository.UpdateAsync(round);
                                }
                                else
                                {
                                    var newRound = new Round
                                    {
                                        CategoryId = category.Id,
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

                        // Update Awards
                        if (categoryRequest.Awards != null && categoryRequest.Awards.Any())
                        {
                            foreach (var awardRequest in categoryRequest.Awards)
                            {
                                var award = await awardRepository.SingleOrDefaultAsync(a => a.CategoryId == category.Id && a.Id == awardRequest.Id, null, null);
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
                                        CategoryId = category.Id,
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
                        if (categoryRequest.CriteriaGroups != null && categoryRequest.CriteriaGroups.Any())
                        {
                            foreach (var criteriaGroupRequest in categoryRequest.CriteriaGroups)
                            {
                                var criteriaGroup = await criteriaGroupRepository.SingleOrDefaultAsync(g => g.CategoryId == category.Id && g.Id == criteriaGroupRequest.Id, null, null);
                                if (criteriaGroup != null)
                                {
                                    criteriaGroup.Name = criteriaGroupRequest.Name;
                                    criteriaGroup.Description = criteriaGroupRequest.Description;
                                    criteriaGroup.RoundType = criteriaGroupRequest.RoundType;

                                     criteriaGroupRepository.UpdateAsync(criteriaGroup);

                                    // Update Criteria
                                    if (criteriaGroupRequest.Criterias != null && criteriaGroupRequest.Criterias.Any())
                                    {
                                        foreach (var criteriaRequest in criteriaGroupRequest.Criterias)
                                        {
                                            var criteria = await criteriaRepository.SingleOrDefaultAsync(c => c.CriteriaGroupId == criteriaGroup.Id && c.Id == criteriaRequest.Id, null, null);
                                            if (criteria != null)
                                            {
                                                criteria.Name = criteriaRequest.Name;
                                                criteria.Description = criteriaRequest.Description;
                                                criteria.MaxScore = criteriaRequest.MaxScore;
                                                criteria.Weight = criteriaRequest.Weight;
                                                criteria.Order = criteriaRequest.Order;

                                                 criteriaRepository.UpdateAsync(criteria);
                                            }
                                            else
                                            {
                                                var newCriteria = new Criterion
                                                {
                                                    CriteriaGroupId = criteriaGroup.Id,
                                                    Name = criteriaRequest.Name,
                                                    Description = criteriaRequest.Description,
                                                    MaxScore = criteriaRequest.MaxScore,
                                                    Weight = criteriaRequest.Weight,
                                                    Order = criteriaRequest.Order
                                                };
                                                await criteriaRepository.InsertAsync(newCriteria);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    throw new NotFoundException($"CriteriaGroup with ID {criteriaGroupRequest.Id} not found.");
                                }
                            }
                        }

                        // Update RefereeAssignments
                        if (categoryRequest.RefereeAssignments != null && categoryRequest.RefereeAssignments.Any())
                        {
                            foreach (var refereeRequest in categoryRequest.RefereeAssignments)
                            {
                                var refereeAssignment = new RefereeAssignment
                                {
                                    CategoryId = category.Id,
                                    RefereeAccountId = refereeRequest.RefereeAccountId,
                                    AssignedAt = refereeRequest.AssignedAt ?? DateTime.Now,
                                    AssignedBy = refereeRequest.AssignedBy
                                };
                                 refereeAssignmentRepository.UpdateAsync(refereeAssignment);
                            }
                        }
                    }
                    else
                    {
                        throw new NotFoundException($"Category with ID {categoryRequest.Id} not found in Show {id}. Cannot update this Category.");
                    }
                }
            }

            // Handle other entities such as ShowStaff, ShowRule, ShowStatistic, ShowStatus, Sponsor, Ticket
            // Update ShowStaffs
            if (updateShowRequest.ShowStaffs != null && updateShowRequest.ShowStaffs.Any())
            {
                foreach (var staffRequest in updateShowRequest.ShowStaffs)
                {
                    var staff = await showStaffRepository.SingleOrDefaultAsync(s => s.Id == staffRequest.Id, null, null);
                    if (staff != null)
                    {
                        staff.AccountId = staffRequest.AccountId;
                        staff.AssignedBy = staffRequest.AssignedBy;
                        staff.AssignedAt = staffRequest.AssignedAt ?? staff.AssignedAt;
                         showStaffRepository.UpdateAsync(staff);
                    }
                    else
                    {
                        throw new NotFoundException($"ShowStaff with ID {staffRequest.Id} not found.");
                    }
                }
            }

            // Update ShowRules
            if (updateShowRequest.ShowRules != null && updateShowRequest.ShowRules.Any())
            {
                foreach (var ruleRequest in updateShowRequest.ShowRules)
                {
                    var rule = await showRuleRepository.SingleOrDefaultAsync(r => r.Id == ruleRequest.Id, null, null);
                    if (rule != null)
                    {
                        rule.Title = ruleRequest.Title;
                        rule.Content = ruleRequest.Content;
                         showRuleRepository.UpdateAsync(rule);
                    }
                    else
                    {
                        throw new NotFoundException($"ShowRule with ID {ruleRequest.Id} not found.");
                    }
                }
            }

            // Update ShowStatistics
            if (updateShowRequest.ShowStatistics != null && updateShowRequest.ShowStatistics.Any())
            {
                foreach (var statisticRequest in updateShowRequest.ShowStatistics)
                {
                    var statistic = await showStatisticRepository.SingleOrDefaultAsync(s => s.Id == statisticRequest.Id, null, null);
                    if (statistic != null)
                    {
                        statistic.MetricName = statisticRequest.MetricName;
                        statistic.MetricValue = statisticRequest.MetricValue;
                         showStatisticRepository.UpdateAsync(statistic);
                    }
                    else
                    {
                        throw new NotFoundException($"ShowStatistic with ID {statisticRequest.Id} not found.");
                    }
                }
            }

            // Update ShowStatuses
            if (updateShowRequest.ShowStatuses != null && updateShowRequest.ShowStatuses.Any())
            {
                foreach (var statusRequest in updateShowRequest.ShowStatuses)
                {
                    var status = await showStatusRepository.SingleOrDefaultAsync(s => s.Id == statusRequest.Id, null, null);
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
                        throw new NotFoundException($"ShowStatus with ID {statusRequest.Id} not found.");
                    }
                }
            }

            // Update Sponsors
            if (updateShowRequest.Sponsors != null && updateShowRequest.Sponsors.Any())
            {
                foreach (var sponsorRequest in updateShowRequest.Sponsors)
                {
                    var sponsor = await sponsorRepository.SingleOrDefaultAsync(s => s.Id == sponsorRequest.Id, null, null);
                    if (sponsor != null)
                    {
                        sponsor.Name = sponsorRequest.Name;
                        sponsor.LogoUrl = sponsorRequest.LogoUrl;
                         sponsorRepository.UpdateAsync(sponsor);
                    }
                    else
                    {
                        throw new NotFoundException($"Sponsor with ID {sponsorRequest.Id} not found.");
                    }
                }
            }

            // Update Tickets
            if (updateShowRequest.Tickets != null && updateShowRequest.Tickets.Any())
            {
                foreach (var ticketRequest in updateShowRequest.Tickets)
                {
                    var ticket = await ticketRepository.SingleOrDefaultAsync(t => t.Id == ticketRequest.Id, null, null);
                    if (ticket != null)
                    {
                        ticket.TicketType = ticketRequest.TicketType;
                        ticket.Price = ticketRequest.Price;
                        ticket.AvailableQuantity = ticketRequest.AvailableQuantity;
                         ticketRepository.UpdateAsync(ticket);
                    }
                    else
                    {
                        throw new NotFoundException($"Ticket with ID {ticketRequest.Id} not found.");
                    }
                }
            }

            // Commit all changes to the database
            await _unitOfWork.CommitAsync();
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
                predicate: s => s.ShowId == showId,
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
