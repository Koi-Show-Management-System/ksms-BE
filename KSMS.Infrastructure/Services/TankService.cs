using KSMS.Application.Repositories;
using KSMS.Domain.Dtos.Requests.Tank;
using KSMS.Domain.Dtos.Responses.Tank;
using KSMS.Domain.Entities;
using KSMS.Infrastructure.Database;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KSMS.Application.Services;
using KSMS.Domain.Exceptions;
using Mapster;
using KSMS.Domain.Pagination;
using System.Linq.Expressions;
using KSMS.Domain.Enums;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace KSMS.Infrastructure.Services
{
    public class TankService : BaseService<TankService>, ITankService
    {
        public TankService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<TankService> logger, IHttpContextAccessor httpContextAccessor)
            : base(unitOfWork, logger, httpContextAccessor)
        {
        }

        public async Task CreateTankAsync(CreateTankRequest request)
        {
            var tankRepository = _unitOfWork.GetRepository<Tank>();
            var category = await _unitOfWork.GetRepository<CompetitionCategory>().SingleOrDefaultAsync(predicate: x=> x.Id == request.CompetitionCategoryId);
            if (category == null)
            {
                throw new NotFoundException("Category not found");
            }
            if (category.HasTank == false)
            {
                throw new BadRequestException("This category does not require tanks.");
            }
            var existingTank = await tankRepository.SingleOrDefaultAsync(
                predicate: t => t.Name == request.Name);
            
            if (existingTank != null)
            {
                throw new BadRequestException($"A tank with the name '{request.Name}' already exists in this KoiShow.");
            }
            var tank = request.Adapt<Tank>();
            tank.CreatedBy = GetIdFromJwt();
            await tankRepository.InsertAsync(tank);
            await _unitOfWork.CommitAsync();
        }
        public async Task<int> GetCurrentFishCount(Guid tankId)
        {
            var regisRoundRepository = _unitOfWork.GetRepository<RegistrationRound>();

            // Đếm số lượng cá hiện có trong hồ
            return await regisRoundRepository.CountAsync(
                predicate: rr => rr.TankId == tankId
            );
        }
        public async Task<bool> IsTankFull(Guid tankId)
        {
            var tankRepository = _unitOfWork.GetRepository<Tank>();
            var regisRoundRepository = _unitOfWork.GetRepository<RegistrationRound>();

            // Lấy thông tin sức chứa của hồ
            var tank = await tankRepository.SingleOrDefaultAsync(
                predicate: t => t.Id == tankId
            );

            if (tank == null)
            {
                throw new NotFoundException($"Tank with ID {tankId} not found.");
            }

            // Đếm số lượng cá hiện có trong hồ
            int currentFishCount = await regisRoundRepository.CountAsync(
                predicate: rr => rr.TankId == tankId
            );

            // Kiểm tra nếu số lượng cá đã đạt hoặc vượt quá sức chứa
            return currentFishCount >= tank.Capacity;
        }

        public async Task<Paginate<TankResponse>> GetPagedTanksByCategoryIdAsync(Guid competitionCategoryId, int page, int size)
        {
            var tankRepository = _unitOfWork.GetRepository<Tank>();

            Expression<Func<Tank, bool>> filterQuery = tank => tank.CompetitionCategoryId == competitionCategoryId;

            var pagedTanks = await tankRepository.GetPagingListAsync(
                predicate: filterQuery,
                orderBy: query => query.OrderBy(t => t.Name),
                page: page,
                size: size
            );

            return pagedTanks.Adapt<Paginate<TankResponse>>();
        }

        public async Task UpdateTankAsync(Guid id, UpdateTankRequest request)
        {
            string role = GetRoleFromJwt();
            var tankRepository = _unitOfWork.GetRepository<Tank>();

            var existingTank = await tankRepository.SingleOrDefaultAsync(
                predicate: t => t.Id == id
            );

            if (existingTank == null)
            {
                throw new NotFoundException($"Tank with ID {id} not found.");
            }
            if (request.Name != existingTank.Name)
            {
                var duplicateTank = await tankRepository.SingleOrDefaultAsync(
                    predicate: t => t.Name == request.Name && t.Id != id
                );

                if (duplicateTank != null)
                {
                    throw new BadRequestException($"A tank with the name '{request.Name}' already exists.");
                }
            }
            existingTank.Name = request.Name ?? existingTank.Name;
            existingTank.Capacity = request.Capacity;
            existingTank.WaterType = request.WaterType ?? existingTank.WaterType;
            existingTank.Temperature = request.Temperature ?? existingTank.Temperature;
            existingTank.Phlevel = request.Phlevel ?? existingTank.Phlevel;
            existingTank.Size = request.Size ?? existingTank.Size;
            existingTank.Location = request.Location ?? existingTank.Location;
            existingTank.Status = request.Status ?? existingTank.Status;

            tankRepository.UpdateAsync(existingTank);
            await _unitOfWork.CommitAsync();
        }

        public async Task UpdateTankStatusAsync(Guid id, TankStatus status)
        {
            
            var tankRepository = _unitOfWork.GetRepository<Tank>();

            var existingTank = await tankRepository.SingleOrDefaultAsync(
                predicate: t => t.Id == id
            );

            if (existingTank == null)
            {
                throw new NotFoundException($"Tank with ID {id} not found.");
            }

            existingTank.Status = status.ToString().ToLower();

            tankRepository.UpdateAsync(existingTank);
            await _unitOfWork.CommitAsync();
        }
    }
}
