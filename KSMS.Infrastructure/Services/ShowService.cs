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
                if (createShowRequest.ShowStaffs != null && createShowRequest.ShowStaffs.Any())
                {
                    foreach (var staffRequest in createShowRequest.ShowStaffs)
                    {
                        var showStaff = staffRequest.Adapt<ShowStaff>();
                        showStaff.ShowId = showId;
                        await showStaffRepository.InsertAsync(showStaff);
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

        public Task<IEnumerable<ShowResponse>> GetAllShowsAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<ShowResponse> GetShowByIdAsync(Guid id)
        {
           
            var showRepository = _unitOfWork.GetRepository<Show>();

           
            var show = await showRepository.SingleOrDefaultAsync(
                predicate: s => s.Id == id,
                include: s => s.Include(s => s.ShowStatuses) 
                              .Include(s => s.ShowStaffs) 
                              .Include(s => s.ShowRules) 
                              .Include(s => s.ShowStatistics) 
                              .Include(s => s.Sponsors) 
                              .Include(s => s.Tickets) 
            );

          
            if (show == null)
            {
                throw new NotFoundException("Show not found.");
            }

          
            var categories = await _unitOfWork.GetRepository<Category>().GetListAsync(
                c => c.ShowId == id,  
                null,  
                c => c.Include(c => c.Rounds)  
                     .Include(c => c.Awards)  
                     .Include(c => c.CriteriaGroups)  
                     .Include(c => c.RefereeAssignments) 
            );

            
            foreach (var category in categories)
            {
                foreach (var criteriaGroup in category.CriteriaGroups)
                {
                   
                    if (criteriaGroup.Criteria == null)
                    {
                        criteriaGroup.Criteria = new List<Criterion>();
                    }

                  
                    var criterias = await _unitOfWork.GetRepository<Criterion>().GetListAsync(
                        c => c.CriteriaGroupId == criteriaGroup.Id, null, null
                    );

                    
                    criteriaGroup.Criteria = criterias.ToList(); 
                }
            }

            
            show.Categories = categories.ToList();

            
            var showResponse = show.Adapt<ShowResponse>();

            
            return showResponse;
        }

        public Task PatchShowStatusAsync(Guid id, string statusName)
        {
            throw new NotImplementedException();
        }

        public Task UpdateShowAsync(Guid id, CreateShowRequest request)
        {
            throw new NotImplementedException();
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
