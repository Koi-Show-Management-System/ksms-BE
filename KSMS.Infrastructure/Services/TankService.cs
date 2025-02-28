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

namespace KSMS.Infrastructure.Services
{
    public class TankService : ITankService
    {
        private readonly IUnitOfWork<KoiShowManagementSystemContext> _unitOfWork;
        private readonly ILogger<TankService> _logger;

        public TankService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<TankService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
         
        public async Task CreateTankAsync(CreateTankRequest request)
        {
            var tankRepository = _unitOfWork.GetRepository<Tank>();

           
            var existingTank = await tankRepository.SingleOrDefaultAsync(
                predicate: t => t.Name == request.Name && t.KoiShowId == request.KoiShowId
            );


            if (existingTank != null)
            {
                throw new BadRequestException($"A tank with the name '{request.Name}' already exists in this KoiShow.");
            }

           
            var tank = request.Adapt<Tank>();

             
            var createdTank = await tankRepository.InsertAsync(tank);
            await _unitOfWork.CommitAsync();

            
            
        }

        public async Task<Paginate<TankResponse>> GetPagedTanksByKoiShowIdAsync(Guid koiShowId, int page, int size)
        {
            var tankRepository = _unitOfWork.GetRepository<Tank>();

            // Lọc theo KoiShowId
            Expression<Func<Tank, bool>> filterQuery = tank => tank.KoiShowId == koiShowId;

            // Gọi repository với phân trang
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
            var tankRepository = _unitOfWork.GetRepository<Tank>();

           
            var existingTank = await tankRepository.SingleOrDefaultAsync(
                predicate: t => t.Id == id
            );

            if (existingTank == null)
            {
                throw new NotFoundException($"Tank with ID {id} not found.");
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


        //public async Task<List<TankResponse>> GetTanksByKoiShowIdAsync(Guid koiShowId)
        //{
        //    var tankRepository = _unitOfWork.GetRepository<Tank>();

        //    var tanks = await tankRepository.GetListAsync(
        //        predicate: t => t.KoiShowId == koiShowId
        //    );

        //    return tanks.Adapt<List<TankResponse>>();
        //}
    }
}
