using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.RegistrationRound;
using KSMS.Domain.Dtos.Responses.RegistrationRound;
using KSMS.Domain.Entities;
using KSMS.Domain.Exceptions;
using KSMS.Infrastructure.Database;
using Mapster;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace KSMS.Infrastructure.Services
{
    public class RegistrationRoundService : IRegistrationRoundService
    {
        private readonly IUnitOfWork<KoiShowManagementSystemContext> _unitOfWork;
        private readonly ILogger<RegistrationRoundService> _logger;

        public RegistrationRoundService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<RegistrationRoundService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // Tạo mới một Registration Round
        public async Task<RegistrationRoundResponse> CreateRegistrationRoundAsync(CreateRegistrationRoundRequest request)
        {
            var registrationRoundRepository = _unitOfWork.GetRepository<RegistrationRound>();

            // Kiểm tra nếu vòng đăng ký đã tồn tại
            var existingRegistrationRound = await registrationRoundRepository.SingleOrDefaultAsync(
                predicate: rr => rr.RegistrationId == request.RegistrationId && rr.RoundId == request.RoundId
            );

            if (existingRegistrationRound != null)
            {
                throw new BadRequestException("Registration round already exists for this registration and round.");
            }

            // Chuyển đổi từ DTO sang Entity
            var registrationRound = request.Adapt<RegistrationRound>();

            // Lưu RegistrationRound vào cơ sở dữ liệu
            var createdRegistrationRound = await registrationRoundRepository.InsertAsync(registrationRound);
            await _unitOfWork.CommitAsync();

            return createdRegistrationRound.Adapt<RegistrationRoundResponse>();
        }

     
        //public async Task<RegistrationRoundResponse> GetRegistrationRoundAsync(Guid registrationId, Guid roundId)
        //{
        //    var registrationRoundRepository = _unitOfWork.GetRepository<RegistrationRound>();

            
        //    var registrationRound = await registrationRoundRepository.SingleOrDefaultAsync(
        //        predicate: rr => rr.RegistrationId == registrationId && rr.RoundId == roundId
        //    );

        //    if (registrationRound == null)
        //    {
        //        throw new NotFoundException("Registration round not found.");
        //    }

        //    return registrationRound.Adapt<RegistrationRoundResponse>();
        //}
    }
}
