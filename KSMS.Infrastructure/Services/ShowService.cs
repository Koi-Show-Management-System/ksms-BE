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

            // Lấy đối tượng Show từ cơ sở dữ liệu
            var show = await showRepository.SingleOrDefaultAsync(
                predicate: s => s.Id == id,
                include: query => query.Include(s => s.Categories)  // Lấy Show cùng với Categories
            );

            // Kiểm tra nếu Show không tồn tại
            if (show == null)
            {
                throw new NotFoundException("Show not found.");
            }

            // Hàm kiểm tra và gán giá trị hợp lệ cho DateTime
            DateTime? ValidateDate(DateTime? date)
            {
                if (date.HasValue && date.Value >= new DateTime(1753, 1, 1) && date.Value <= new DateTime(9999, 12, 31))
                {
                    return date;
                }
                return null;  // Trả về null nếu ngày không hợp lệ
            }

            // Kiểm tra và gán các giá trị ngày tháng hợp lệ cho Show
            show.StartDate = ValidateDate(updateShowRequest.StartDate) ?? show.StartDate;
            show.EndDate = ValidateDate(updateShowRequest.EndDate) ?? show.EndDate;
            show.StartExhibitionDate = ValidateDate(updateShowRequest.StartExhibitionDate) ?? show.StartExhibitionDate;
            show.EndExhibitionDate = ValidateDate(updateShowRequest.EndExhibitionDate) ?? show.EndExhibitionDate;

            // Cập nhật thông tin từ updateShowRequest vào show
            updateShowRequest.Adapt(show);
            show.UpdatedAt = DateTime.Now;

            // Cập nhật các Category liên quan
            if (updateShowRequest.Categories != null && updateShowRequest.Categories.Any())
            {
                foreach (var categoryRequest in updateShowRequest.Categories)
                {
                    var category = await categoryRepository.SingleOrDefaultAsync(c => c.Id == categoryRequest.Id && c.ShowId == id, null, null);
                    if (category != null)
                    {
                        categoryRequest.Adapt(category); // Cập nhật Category nếu tìm thấy

                        // Kiểm tra và gán StartTime và EndTime hợp lệ cho Category
                        if (categoryRequest.StartTime.HasValue)
                        {
                            categoryRequest.StartTime = ValidateDate(categoryRequest.StartTime);  // Validate StartTime
                        }
                        else
                        {
                            categoryRequest.StartTime = null; // Keep it null if no valid date is provided
                        }

                        if (categoryRequest.EndTime.HasValue)
                        {
                            categoryRequest.EndTime = ValidateDate(categoryRequest.EndTime);  // Validate EndTime
                        }
                        else
                        {
                            categoryRequest.EndTime = null; // Keep it null if no valid date is provided
                        }
                    }
                }
            }

            // Commit các thay đổi vào cơ sở dữ liệu
            _unitOfWork.GetRepository<Show>().UpdateAsync(show);
            _unitOfWork.GetRepository<Category>().UpdateRange(show.Categories);
            await _unitOfWork.CommitAsync();
        }

        //public async Task UpdateShowAsync(Guid id, UpdateShowRequest updateShowRequest)
        //{   
        //    var showRepository = _unitOfWork.GetRepository<Show>();
        //    var categoryRepository = _unitOfWork.GetRepository<Category>();
        //    var varietyRepository = _unitOfWork.GetRepository<Variety>();
        //    var sponsorRepository = _unitOfWork.GetRepository<Sponsor>();
        //    var showStaffRepository = _unitOfWork.GetRepository<ShowStaff>();
        //    var showRuleRepository = _unitOfWork.GetRepository<ShowRule>();
        //    var showStatusRepository = _unitOfWork.GetRepository<ShowStatus>();
        //    var showStatisticRepository = _unitOfWork.GetRepository<ShowStatistic>();
        //    var ticketRepository = _unitOfWork.GetRepository<Ticket>();
        //    var roundRepository = _unitOfWork.GetRepository<Round>();
        //    var criteriaGroupRepository = _unitOfWork.GetRepository<CriteriaGroup>();
        //    var criteriaRepository = _unitOfWork.GetRepository<Criterion>();
        //    var refereeAssignmentRepository = _unitOfWork.GetRepository<RefereeAssignment>();


        //    var show = await showRepository.SingleOrDefaultAsync(
        //        predicate: s => s.Id == id,
        //        include: query => query.Include(s => s.ShowStatuses)
        //            .Include(s => s.Categories)
        //                .ThenInclude(s => s.Rounds)
        //            .Include(s => s.Categories)
        //                .ThenInclude(s => s.Awards)
        //            .Include(s => s.Categories)
        //                .ThenInclude(s => s.CriteriaGroups)
        //                    .ThenInclude(s => s.Criteria)
        //            .Include(s => s.Categories)
        //                .ThenInclude(s => s.RefereeAssignments)
        //            .Include(s => s.ShowStaffs)
        //            .Include(s => s.ShowRules)
        //            .Include(s => s.ShowStatistics)
        //            .Include(s => s.Sponsors)
        //            .Include(s => s.Tickets)
        //    );

        //    if (show == null)
        //    {
        //        throw new NotFoundException("Show not found.");
        //    }
        //    show.StartDate = updateShowRequest.StartDate.HasValue && updateShowRequest.StartDate.Value >= new DateTime(1753, 1, 1) ? updateShowRequest.StartDate.Value : DateTime.Now;
        //    show.EndDate = updateShowRequest.EndDate.HasValue && updateShowRequest.EndDate.Value >= new DateTime(1753, 1, 1) ? updateShowRequest.EndDate.Value : DateTime.Now.AddDays(1);
        //    show.StartExhibitionDate = updateShowRequest.StartExhibitionDate.HasValue && updateShowRequest.StartExhibitionDate.Value >= new DateTime(1753, 1, 1) ? updateShowRequest.StartExhibitionDate.Value : DateTime.Now.AddDays(2);
        //    show.EndExhibitionDate = updateShowRequest.EndExhibitionDate.HasValue && updateShowRequest.EndExhibitionDate.Value >= new DateTime(1753, 1, 1) ? updateShowRequest.EndExhibitionDate.Value : DateTime.Now.AddDays(3);


        //    updateShowRequest.Adapt(show);
        //    updateShowRequest.Adapt(show).UpdatedAt = DateTime.Now;


        //    if (updateShowRequest.Categories != null && updateShowRequest.Categories.Any())
        //    {
        //        foreach (var categoryRequest in updateShowRequest.Categories)
        //        {
        //            var category = await categoryRepository.SingleOrDefaultAsync(c => c.Id == categoryRequest.Id && c.ShowId == id, null, null);

        //            if (category != null)
        //            {
        //                categoryRequest.Adapt(category);  // Cập nhật Category nếu tìm thấy
        //            }

        //            // Cập nhật các Round liên quan
        //            if (categoryRequest.Rounds != null && categoryRequest.Rounds.Any())
        //            {
        //                foreach (var roundRequest in categoryRequest.Rounds)
        //                {
        //                    var round = await roundRepository.SingleOrDefaultAsync(r => r.CategoryId == category.Id && r.Id == roundRequest.Id, null, null);
        //                    if (round != null)
        //                    {
        //                        roundRequest.Adapt(round);  // Cập nhật Round nếu tìm thấy
        //                    }
        //                    else
        //                    {
        //                        // Thêm mới Round nếu không tồn tại
        //                        var newRound = roundRequest.Adapt<Round>();
        //                        newRound.CategoryId = category.Id;
        //                        await roundRepository.InsertAsync(newRound);
        //                    }
        //                }
        //            }

        //            // Cập nhật CriteriaGroups và Criterias liên quan
        //            if (categoryRequest.CriteriaGroups != null && categoryRequest.CriteriaGroups.Any())
        //            {
        //                foreach (var criteriaGroupRequest in categoryRequest.CriteriaGroups)
        //                {
        //                    var criteriaGroup = await criteriaGroupRepository.SingleOrDefaultAsync(g => g.CategoryId == category.Id && g.Id == criteriaGroupRequest.Id, null, null);
        //                    if (criteriaGroup != null)
        //                    {
        //                        criteriaGroupRequest.Adapt(criteriaGroup);  // Cập nhật CriteriaGroup nếu tìm thấy
        //                    }
        //                    else
        //                    {
        //                        // Thêm mới CriteriaGroup nếu không tồn tại
        //                        var newGroup = criteriaGroupRequest.Adapt<CriteriaGroup>();
        //                        newGroup.CategoryId = category.Id;
        //                        await criteriaGroupRepository.InsertAsync(newGroup);
        //                    }

        //                    // Cập nhật Criteria liên quan trong CriteriaGroup
        //                    if (criteriaGroupRequest.Criterias != null && criteriaGroupRequest.Criterias.Any())
        //                    {
        //                        foreach (var criteriaRequest in criteriaGroupRequest.Criterias)
        //                        {
        //                            var criteria = await criteriaRepository.SingleOrDefaultAsync(c => c.CriteriaGroupId == criteriaGroup.Id && c.Id == criteriaRequest.Id, null, null);
        //                            if (criteria != null)
        //                            {
        //                                criteriaRequest.Adapt(criteria);  // Cập nhật Criteria nếu tìm thấy
        //                            }
        //                            else
        //                            {
        //                                // Thêm mới Criteria nếu không tồn tại
        //                                var newCriteria = criteriaRequest.Adapt<Criterion>();
        //                                newCriteria.CriteriaGroupId = criteriaGroup.Id;
        //                                await criteriaRepository.InsertAsync(newCriteria);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    //// Cập nhật các Sponsors
        //    //if (updateShowRequest.Sponsors != null && updateShowRequest.Sponsors.Any())
        //    //{
        //    //    foreach (var sponsorRequest in updateShowRequest.Sponsors)
        //    //    {
        //    //        var sponsor = await sponsorRepository.SingleOrDefaultAsync(s => s.ShowId == id && s.Id == sponsorRequest.Id, null, null);
        //    //        if (sponsor != null)
        //    //        {
        //    //            sponsorRequest.Adapt(sponsor);  // Cập nhật Sponsor nếu tìm thấy
        //    //        }
        //    //        else
        //    //        {
        //    //            var newSponsor = sponsorRequest.Adapt<Sponsor>();
        //    //            newSponsor.ShowId = id;
        //    //            await sponsorRepository.InsertAsync(newSponsor);  // Thêm mới Sponsor nếu không tồn tại
        //    //        }
        //    //    }
        //    //}

        //    //// Cập nhật ShowStaffs
        //    //if (updateShowRequest.ShowStaffs != null && updateShowRequest.ShowStaffs.Any())
        //    //{
        //    //    foreach (var staffRequest in updateShowRequest.ShowStaffs)
        //    //    {
        //    //        var staff = await showStaffRepository.SingleOrDefaultAsync(s => s.Id == staffRequest.Id && s.ShowId == id, null, null);
        //    //        if (staff != null)
        //    //        {
        //    //            staffRequest.Adapt(staff);  // Cập nhật ShowStaff nếu tìm thấy
        //    //        }
        //    //    }
        //    //}

        //    //// Cập nhật ShowRules
        //    //if (updateShowRequest.ShowRules != null && updateShowRequest.ShowRules.Any())
        //    //{
        //    //    foreach (var ruleRequest in updateShowRequest.ShowRules)
        //    //    {
        //    //        var rule = await showRuleRepository.SingleOrDefaultAsync(r => r.ShowId == id && r.Id == ruleRequest.ShowId, null, null);
        //    //        if (rule != null)
        //    //        {
        //    //            ruleRequest.Adapt(rule);  // Cập nhật ShowRule nếu tìm thấy
        //    //        }
        //    //    }
        //    //}

        //    //// Cập nhật ShowStatuses
        //    //if (updateShowRequest.ShowStatuses != null && updateShowRequest.ShowStatuses.Any())
        //    //{
        //    //    foreach (var statusRequest in updateShowRequest.ShowStatuses)
        //    //    {
        //    //        var status = await showStatusRepository.SingleOrDefaultAsync(s => s.ShowId == id && s.Id == statusRequest.ShowId, null, null);
        //    //        if (status != null)
        //    //        {
        //    //            statusRequest.Adapt(status);  // Cập nhật ShowStatus nếu tìm thấy
        //    //        }
        //    //    }
        //    //}

        //    //// Cập nhật ShowStatistics
        //    //if (updateShowRequest.ShowStatistics != null && updateShowRequest.ShowStatistics.Any())
        //    //{
        //    //    foreach (var statRequest in updateShowRequest.ShowStatistics)
        //    //    {
        //    //        var stat = await showStatisticRepository.SingleOrDefaultAsync(s => s.ShowId == id && s.Id == statRequest.ShowId, null, null);
        //    //        if (stat != null)
        //    //        {
        //    //            statRequest.Adapt(stat);  // Cập nhật ShowStatistic nếu tìm thấy
        //    //        }
        //    //    }
        //    //}

        //    //// Cập nhật Tickets
        //    //if (updateShowRequest.Tickets != null && updateShowRequest.Tickets.Any())
        //    //{
        //    //    foreach (var ticketRequest in updateShowRequest.Tickets)
        //    //    {
        //    //        var ticket = await ticketRepository.SingleOrDefaultAsync(t => t.ShowId == id && t.Id == ticketRequest.ShowId, null, null);
        //    //        if (ticket != null)
        //    //        {
        //    //            ticketRequest.Adapt(ticket);  // Cập nhật Ticket nếu tìm thấy
        //    //        }
        //    //    }
        //    //}

        //    //// Commit các thay đổi vào cơ sở dữ liệu

        //    _unitOfWork.GetRepository<Show>().UpdateAsync(show);
        //    await _unitOfWork.CommitAsync();





        //}

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
