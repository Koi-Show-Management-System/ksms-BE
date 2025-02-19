using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.Show;
using KSMS.Domain.Dtos.Responses.KoiShow;
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
using Microsoft.AspNetCore.Http;
using static KSMS.Infrastructure.Utils.MailUtil;
 

namespace KSMS.Infrastructure.Services
{
    public class ShowService : BaseService<ShowService>, IShowService
    {

        public ShowService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<ShowService> logger, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, httpContextAccessor)
        {
        }
        public async Task<KoiShowResponse> CreateShowAsync(CreateShowRequest createShowRequest)
        {
            // Lấy các repository cần thiết với tên mới
            var showRepository = _unitOfWork.GetRepository<KoiShow>();
            var categoryRepository = _unitOfWork.GetRepository<CompetitionCategory>();
            var sponsorRepository = _unitOfWork.GetRepository<Sponsor>();
            var showStaffRepository = _unitOfWork.GetRepository<ShowStaff>();
            var showRuleRepository = _unitOfWork.GetRepository<ShowRule>();
            var showStatusRepository = _unitOfWork.GetRepository<ShowStatus>();
            var ticketRepository = _unitOfWork.GetRepository<Ticket>();
            var roundRepository = _unitOfWork.GetRepository<Round>();
            var criteriaGroupRepository = _unitOfWork.GetRepository<CriteriaCompetitionCategory>();
            var criteriaRepository = _unitOfWork.GetRepository<Criterion>();
            var refereeAssignmentRepository = _unitOfWork.GetRepository<RefereeAssignment>();
            var accountRepository = _unitOfWork.GetRepository<Account>();
            var categoryVarietyRepository = _unitOfWork.GetRepository<CategoryVariety>();
            var errorTypeRepository = _unitOfWork.GetRepository<ErrorType>();
            var scoreDetailErrorRepository = _unitOfWork.GetRepository<ScoreDetailError>();
            var awardRepository = _unitOfWork.GetRepository<Award>();
            var varietyRepository = _unitOfWork.GetRepository<Variety>();

            // Validate the input data
            if (string.IsNullOrWhiteSpace(createShowRequest.Name))
            {
                throw new BadRequestException("Show name cannot be empty.");
            }

            if (createShowRequest.StartDate >= createShowRequest.EndDate)
            {
                throw new BadRequestException("Start date must be earlier than end date.");
            }

            // Create the Show entity
            var newShow = createShowRequest.Adapt<KoiShow>();
            newShow.CreatedAt = DateTime.UtcNow;

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Save the Show
                var createdShow = await showRepository.InsertAsync(newShow);
                await _unitOfWork.CommitAsync();
                var showId = createdShow.Id;

                // Process Categories and related entities
                if (createShowRequest.Categories != null && createShowRequest.Categories.Any())
                {
                    foreach (var categoryRequest in createShowRequest.Categories)
                    {
                        // Create Category
                        var category = categoryRequest.Adapt<CompetitionCategory>();
                        category.KoiShowId = showId;
                        var createdCategory = await categoryRepository.InsertAsync(category);
                        await _unitOfWork.CommitAsync();

                        // Process CategoryVariety (CategoryVarietyRequest)
                        if (categoryRequest.CategoryVarietys != null && categoryRequest.CategoryVarietys.Any())
                        {
                            foreach (var categoryVarietyRequest in categoryRequest.CategoryVarietys)
                            {
                                // Ensure Variety exists or create a new one if not found
                                var variety = await varietyRepository.SingleOrDefaultAsync(v => v.Id == categoryVarietyRequest.VarietyId, null, null);
                                if (variety == null)
                                {
                                    // Variety doesn't exist, create a new Variety
                                    variety = new Variety
                                    {
                                        Id = categoryVarietyRequest.VarietyId,  // Set ID from request
                                        Name = categoryVarietyRequest.Variety.Name,
                                        Description = categoryVarietyRequest.Variety.Description
                                    };

                                    // Add new Variety to the database
                                    await varietyRepository.InsertAsync(variety);
                                    await _unitOfWork.CommitAsync(); // Save to the database
                                }

                                // Create CategoryVariety entity
                                var categoryVariety = new CategoryVariety
                                {
                                    CompetitionCategoryId = createdCategory.Id,
                                    VarietyId = variety.Id // Use the newly created or existing VarietyId
                                };

                                // Save CategoryVariety to database
                                await categoryVarietyRepository.InsertAsync(categoryVariety);
                                await _unitOfWork.CommitAsync();
                            }
                        }

                        // Process CriteriaGroups and Criterias for Category
                        if (categoryRequest.CriteriaGroups != null && categoryRequest.CriteriaGroups.Any())
                        {
                            foreach (var groupRequest in categoryRequest.CriteriaGroups)
                            {
                                var group = groupRequest.Adapt<CriteriaCompetitionCategory>();
                                group.CompetitionCategoryId = createdCategory.Id;

                                // Ensure Criterion exists or create a new one if not found
                                var criterion = await criteriaRepository.SingleOrDefaultAsync(c => c.Id == groupRequest.CriteriaId, null,null);
                                if (criterion == null)
                                {
                                    // Criterion doesn't exist, create a new Criterion
                                    criterion = new Criterion
                                    {
                                        Id = groupRequest.CriteriaId,  // Set ID from request
                                        Name = groupRequest.Criterias.Name,
                                        Description = groupRequest.Criterias.Description
                                    };

                                    // Add new Criterion to the database
                                    await criteriaRepository.InsertAsync(criterion);
                                    await _unitOfWork.CommitAsync(); // Save to the database
                                }

                                // Link CriteriaGroup to the created or existing Criterion
                                group.CriteriaId = criterion.Id;

                                // Save CriteriaGroup to database
                                await criteriaGroupRepository.InsertAsync(group);
                                await _unitOfWork.CommitAsync();
                            }
                        }

                        // Process Ticket Types (TicketTypeRequest) associated with the show
                        if (createShowRequest.Tickettypes != null && createShowRequest.Tickettypes.Any())
                        {
                            foreach (var ticketTypeRequest in createShowRequest.Tickettypes)
                            {
                                var ticketType = ticketTypeRequest.Adapt<TicketType>();
                                ticketType.KoiShowId = showId;

                                // Create TicketType and save it to the database
                                await _unitOfWork.GetRepository<TicketType>().InsertAsync(ticketType);
                                await _unitOfWork.CommitAsync();
                            }
                        }

                        // Process Rounds for Category
                        if (categoryRequest.Rounds != null && categoryRequest.Rounds.Any())
                        {
                            foreach (var roundRequest in categoryRequest.Rounds)
                            {
                                var round = roundRequest.Adapt<Round>();
                                round.CompetitionCategoriesId = createdCategory.Id;
                                await roundRepository.InsertAsync(round);
                            }
                        }

                        // Process Referee Assignments for Category
                        if (categoryRequest.RefereeAssignments != null && categoryRequest.RefereeAssignments.Any())
                        {
                            foreach (var refereeAssignmentRequest in categoryRequest.RefereeAssignments)
                            {
                                var refereeAssignment = refereeAssignmentRequest.Adapt<RefereeAssignment>();
                                refereeAssignment.CompetitionCategoryId = createdCategory.Id; // Link RefereeAssignment with Category

                                // Save RefereeAssignment to the database
                                await refereeAssignmentRepository.InsertAsync(refereeAssignment);
                                await _unitOfWork.CommitAsync();
                            }
                        }

                        // Process Awards for Category
                        if (categoryRequest.Awards != null && categoryRequest.Awards.Any())
                        {
                            foreach (var awardRequest in categoryRequest.Awards)
                            {
                                var award = awardRequest.Adapt<Award>();
                                award.CompetitionCategoriesId = createdCategory.Id; // Link the award with the category

                                // Save Award to the database
                                await awardRepository.InsertAsync(award);
                                await _unitOfWork.CommitAsync();
                            }
                        }
                    }
                }

                // Commit the transaction
                await _unitOfWork.CommitAsync();
                await transaction.CommitAsync();

                // Return the created show response
                return createdShow.Adapt<KoiShowResponse>();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Failed to create show and related data.", ex);
            }
        }

        public async Task<IEnumerable<KoiShowResponse>> GetAllShowsAsync()
        {
            var showRepository = _unitOfWork.GetRepository<KoiShow>();

            var shows = await showRepository.GetListAsync(
                predicate: null,
                orderBy: q => q.OrderBy(s => s.Name),
                include: query => query.Include(s => s.ShowStatuses)
                .Include(s => s.CompetitionCategories)
                    .Include(s => s.CompetitionCategories)
                        .ThenInclude(s => s.Rounds)
                    .Include(s => s.CompetitionCategories)
                        .ThenInclude(s => s.Awards)
                    .Include(s => s.CompetitionCategories)
                        .ThenInclude(s => s.CriteriaCompetitionCategories)
                            .ThenInclude(s => s.Criteria)
                    .Include(s => s.CompetitionCategories)
                        .ThenInclude(s => s.RefereeAssignments)
                            .ThenInclude(s => s.RefereeAccount)
                    .Include(s => s.CompetitionCategories)
                        .ThenInclude(s => s.Registrations)
                     .Include(s => s.CompetitionCategories)
                        .ThenInclude(s => s.CategoryVarieties)
                            .ThenInclude(s => s.Variety)
                        
                    .Include(s => s.CompetitionCategories)
                        .ThenInclude(s => s.RefereeAssignments)
                            .ThenInclude(s => s.AssignedByNavigation)
                               
                    .Include(s => s.ShowStaffs)
                        .ThenInclude(s => s.AssignedByNavigation)
                           
                    .Include(s => s.ShowStaffs)
                        .ThenInclude(s => s.Account)
                        
                    .Include(s => s.ShowRules)
                    .Include(s => s.Sponsors)
                    .Include(s => s.TicketTypes) 
                    .Include(s => s.CompetitionCategories)
                        .ThenInclude(s => s.CriteriaCompetitionCategories)
                            .ThenInclude(c => c.Criteria)
                                .ThenInclude(e => e.ErrorTypes)
                              
            );

            var KoiShowResponses = shows.Select(show => show.Adapt<KoiShowResponse>());

            return KoiShowResponses;
        }

        public async Task<KoiShowResponse> GetShowByIdAsync(Guid id)
        {
            var showRepository = _unitOfWork.GetRepository<KoiShow>();
                
            var show = await showRepository.SingleOrDefaultAsync(
                 predicate: s => s.Id == id,
                orderBy: q => q.OrderBy(s => s.Name),
                include: query => query.Include(s => s.ShowStatuses)
                .Include(s => s.CompetitionCategories)
                    .Include(s => s.CompetitionCategories)
                        .ThenInclude(s => s.Rounds)
                    .Include(s => s.CompetitionCategories)
                        .ThenInclude(s => s.Awards)
                    .Include(s => s.CompetitionCategories)
                        .ThenInclude(s => s.CriteriaCompetitionCategories)
                            .ThenInclude(s => s.Criteria)
                    .Include(s => s.CompetitionCategories)
                        .ThenInclude(s => s.RefereeAssignments)
                            .ThenInclude(s => s.RefereeAccount)
                    .Include(s => s.CompetitionCategories)
                        .ThenInclude(s => s.Registrations)
                     .Include(s => s.CompetitionCategories)
                        .ThenInclude(s => s.CategoryVarieties)
                            .ThenInclude(s => s.Variety)

                    .Include(s => s.CompetitionCategories)
                        .ThenInclude(s => s.RefereeAssignments)
                            .ThenInclude(s => s.AssignedByNavigation)

                    .Include(s => s.ShowStaffs)
                        .ThenInclude(s => s.AssignedByNavigation)

                    .Include(s => s.ShowStaffs)
                        .ThenInclude(s => s.Account)

                    .Include(s => s.ShowRules)
                    .Include(s => s.Sponsors)
                    .Include(s => s.TicketTypes)
                    .Include(s => s.CompetitionCategories)
                        .ThenInclude(s => s.CriteriaCompetitionCategories)
                            .ThenInclude(c => c.Criteria)
                                .ThenInclude(e => e.ErrorTypes)


            );

            if (show == null)
            {
                throw new NotFoundException("Show not found.");
            }

            return show.Adapt<KoiShowResponse>();
        }


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

            // Fetch the show from the repository
            var show = await showRepository.SingleOrDefaultAsync(s => s.Id == id, null, null);
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
            if (updateShowRequest.Categories != null && updateShowRequest.Categories.Any())
            {
                foreach (var categoryRequest in updateShowRequest.Categories)
                {
                    var category = await categoryRepository.SingleOrDefaultAsync(c => c.Id == categoryRequest.Id && c.KoiShowId == id, null, null);
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
                        if (categoryRequest.CategoryVarietys != null && categoryRequest.CategoryVarietys.Any())
                        {
                            foreach (var categoryVarietyRequest in categoryRequest.CategoryVarietys)
                            {
                                var variety = await varietyRepository.SingleOrDefaultAsync(v => v.Id == categoryVarietyRequest.VarietyId, null, null);
                                if (variety == null)
                                {
                                    variety = new Variety
                                    {
                                        Id = categoryVarietyRequest.VarietyId,
                                        Name = categoryVarietyRequest.Variety.Name,
                                        Description = categoryVarietyRequest.Variety.Description
                                    };
                                    await varietyRepository.InsertAsync(variety);
                                }

                                var categoryVariety = new CategoryVariety
                                {
                                    CompetitionCategoryId = category.Id,
                                    VarietyId = variety.Id
                                };
                                await _unitOfWork.GetRepository<CategoryVariety>().InsertAsync(categoryVariety);
                            }
                        }

                        // Update Awards
                        if (categoryRequest.Awards != null && categoryRequest.Awards.Any())
                        {
                            foreach (var awardRequest in categoryRequest.Awards)
                            {
                                var award = await awardRepository.SingleOrDefaultAsync(a => a.CompetitionCategoriesId == category.Id && a.Id == awardRequest.Id, null, null);
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
                        if (categoryRequest.CriteriaGroups != null && categoryRequest.CriteriaGroups.Any())
                        {
                            foreach (var criteriaGroupRequest in categoryRequest.CriteriaGroups)
                            {
                                var criteriaGroup = await criteriaGroupRepository.SingleOrDefaultAsync(g => g.CompetitionCategoryId == category.Id && g.Id == criteriaGroupRequest.Id, null, null);
                                if (criteriaGroup != null)
                                {
                                    criteriaGroup.RoundType = criteriaGroupRequest.RoundType;
                                    criteriaGroup.CriteriaId = criteriaGroupRequest.CriteriaId;
                                    criteriaGroup.RoundType = criteriaGroupRequest.RoundType;
                                    criteriaGroup.Order = criteriaGroupRequest.Order;
                                    criteriaGroupRepository.UpdateAsync(criteriaGroup);

                                    var criterion = await criteriaRepository.SingleOrDefaultAsync(c => c.Id == criteriaGroupRequest.Criterias.Id, null, null);
                                    if (criterion == null)
                                    {
                                        criterion = new Criterion
                                        {
                                            Id = criteriaGroupRequest.Criterias.Id,
                                            Name = criteriaGroupRequest.Criterias.Name,
                                            Description = criteriaGroupRequest.Criterias.Description,
                                          //  MaxScore = criteriaGroupRequest.Criterias.MaxScore ?? 0,
                                        //    Weight = criteriaGroupRequest.Criterias.Weight ?? 0,
                                            Order = criteriaGroupRequest.Criterias.Order ?? 0
                                        };
                                        await criteriaRepository.InsertAsync(criterion);
                                    }

                                    criteriaGroup.CriteriaId = criterion.Id;
                                     criteriaGroupRepository.UpdateAsync(criteriaGroup);
                                }
                            }
                        }

                        // Update Rounds
                        if (categoryRequest.Rounds != null && categoryRequest.Rounds.Any())
                        {
                            foreach (var roundRequest in categoryRequest.Rounds)
                            {
                                var round = await roundRepository.SingleOrDefaultAsync(r => r.CompetitionCategoriesId == category.Id && r.Id == roundRequest.Id, null, null);
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
                        if (categoryRequest.RefereeAssignments != null && categoryRequest.RefereeAssignments.Any())
                        {
                            foreach (var refereeRequest in categoryRequest.RefereeAssignments)
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
            if (updateShowRequest.ShowStaffs != null && updateShowRequest.ShowStaffs.Any())
            {
                foreach (var staffRequest in updateShowRequest.ShowStaffs)
                {
                    var staff = await showStaffRepository.SingleOrDefaultAsync(s => s.Id == staffRequest.Id && s.KoiShowId == id, null, null);

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
            if (updateShowRequest.Tickets != null && updateShowRequest.Tickets.Any())
            {
                foreach (var ticketRequest in updateShowRequest.Tickets)
                {
                    var ticket = await ticketRepository.SingleOrDefaultAsync(t => t.Id == ticketRequest.Id && t.KoiShowId == id, null, null);
                    if (ticket != null)
                    {
                        ticket.TicketType1 = ticketRequest.TicketType1;
                        ticket.Price = ticketRequest.Price;
                        ticket.AvailableQuantity = ticketRequest.AvailableQuantity;
                         ticketRepository.UpdateAsync(ticket);
                    }
                    else
                    {
                        var newTicket = new TicketType
                        {
                            KoiShowId = id,
                            TicketType1 = ticketRequest.TicketType1,
                            Price = ticketRequest.Price,
                            AvailableQuantity = ticketRequest.AvailableQuantity
                        };
                        await ticketRepository.InsertAsync(newTicket);
                    }
                }
            }

            // Update Sponsors
            if (updateShowRequest.Sponsors != null && updateShowRequest.Sponsors.Any())
            {
                foreach (var sponsorRequest in updateShowRequest.Sponsors)
                {
                    var sponsor = await sponsorRepository.SingleOrDefaultAsync(s => s.Id == sponsorRequest.Id && s.KoiShowId == id, null, null);
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
            if (updateShowRequest.ShowRules != null && updateShowRequest.ShowRules.Any())
            {
                foreach (var ruleRequest in updateShowRequest.ShowRules)
                {
                    var rule = await showRuleRepository.SingleOrDefaultAsync(r => r.Id == ruleRequest.Id && r.KoiShowId == id, null, null);
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
            if (updateShowRequest.ShowStatuses != null && updateShowRequest.ShowStatuses.Any())
            {
                foreach (var statusRequest in updateShowRequest.ShowStatuses)
                {
                    var status = await showStatusRepository.SingleOrDefaultAsync(s => s.Id == statusRequest.Id && s.KoiShowId == id, null, null);
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
