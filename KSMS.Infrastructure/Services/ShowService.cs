using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.Show;
using KSMS.Domain.Dtos.Responses.Show;
using KSMS.Domain.Entities;
using KSMS.Domain.Exceptions;
using KSMS.Infrastructure.Database;
using Mapster;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Infrastructure.Services
{
    public class ShowService : BaseService<ShowService>, IShowService
    {
        public ShowService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<ShowService> logger) : base(unitOfWork, logger)
        {
        }

        public async Task<ShowResponse> CreateShowAsync(CreateShowRequest createShowRequest)
        {
            var showRepository = _unitOfWork.GetRepository<Show>();
            var categoryRepository = _unitOfWork.GetRepository<Category>();
            var sponsorRepository = _unitOfWork.GetRepository<Sponsor>();
            var showStaffRepository = _unitOfWork.GetRepository<ShowStaff>();
            var showRuleRepository = _unitOfWork.GetRepository<ShowRule>();
            var showStatusRepository = _unitOfWork.GetRepository<ShowStatus>();
            var showStatisticRepository = _unitOfWork.GetRepository<ShowStatistic>();
            var ticketRepository = _unitOfWork.GetRepository<Ticket>();
            var roundRepository = _unitOfWork.GetRepository<Round>(); // Repository xử lý Round

            if (string.IsNullOrWhiteSpace(createShowRequest.Name))
            {
                throw new BadRequestException("Show name cannot be empty.");
            }

            if (createShowRequest.StartDate >= createShowRequest.EndDate)
            {
                throw new BadRequestException("Start date must be earlier than end date.");
            }

            var newShow = createShowRequest.Adapt<Show>();
            newShow.CreatedAt = DateTime.UtcNow;

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var createdShow = await showRepository.InsertAsync(newShow);
                await _unitOfWork.CommitAsync();

                var showId = createdShow.Id;

                // Xử lý Categories và tạo Round liên quan đến Category
                if (createShowRequest.Categories != null && createShowRequest.Categories.Any())
                {
                    foreach (var categoryRequest in createShowRequest.Categories)
                    {
                        var category = categoryRequest.Adapt<Category>();
                        category.ShowId = showId;
                        var createdCategory = await categoryRepository.InsertAsync(category);

                        // Tạo các Round liên kết với Category
                        if (categoryRequest.Rounds != null && categoryRequest.Rounds.Any())
                        {
                            foreach (var roundRequest in categoryRequest.Rounds)
                            {
                                var round = roundRequest.Adapt<Round>();
                                round.CategoryId = createdCategory.Id; // Liên kết với CategoryId
                                await roundRepository.InsertAsync(round);
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

                await _unitOfWork.CommitAsync();
                await transaction.CommitAsync();

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

        public Task<ShowResponse> GetShowByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task PatchShowStatusAsync(Guid id, string statusName)
        {
            throw new NotImplementedException();
        }

        public Task UpdateShowAsync(Guid id, CreateShowRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
