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

        // Tạo mới một hồ
        public async Task<TankResponse> CreateTankAsync(TankRequest request)
        {
            var tankRepository = _unitOfWork.GetRepository<Tank>();

            // Kiểm tra nếu đã tồn tại hồ có tên giống vậy trong hệ thống
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

            
            return createdTank.Adapt<TankResponse>();
        }

      
        public async Task<List<TankResponse>> GetTanksByKoiShowIdAsync(Guid koiShowId)
        {
            var tankRepository = _unitOfWork.GetRepository<Tank>();

            var tanks = await tankRepository.GetListAsync(
                predicate: t => t.KoiShowId == koiShowId
            );

            return tanks.Adapt<List<TankResponse>>();
        }
    }
}
