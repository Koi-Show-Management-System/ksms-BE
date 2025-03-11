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
using KSMS.Domain.Pagination;
using Microsoft.EntityFrameworkCore;

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

        public async Task<Paginate<GetPageRegistrationRoundResponse>> GetPageRegistrationRound(Guid roundId, int page, int size)
        {
            var round = await _unitOfWork.GetRepository<Round>()
                .SingleOrDefaultAsync(predicate: x => x.Id == roundId);
            if (round == null)
            {
                throw new NotFoundException("Round not found");
            }

            var registrationRounds = await _unitOfWork.GetRepository<RegistrationRound>().GetPagingListAsync(
                predicate: x => x.RoundId == roundId,
                include: query => query.Include(x => x.RoundResults)
                    .Include(x => x.Tank),
                page: page, 
                size: size);
            var response = registrationRounds.Adapt<Paginate<GetPageRegistrationRoundResponse>>();
            foreach (var registrationRound in response.Items)
            {
                var registrationRoundEntity = registrationRounds.Items.FirstOrDefault(x => x.Id == registrationRound.Id);
                registrationRound.TankName = registrationRoundEntity?.Tank?.Name;
            }
            return response;
        }
    }
}
