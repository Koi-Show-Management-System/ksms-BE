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
using System.Linq.Expressions;
using System.Threading.Tasks;
using KSMS.Application.Extensions;
using KSMS.Domain.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace KSMS.Infrastructure.Services
{
    public class RegistrationRoundService : BaseService<RegistrationRoundService>,IRegistrationRoundService
    {
        public RegistrationRoundService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<RegistrationRoundService> logger, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, httpContextAccessor)
        {
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
            var role = GetRoleFromJwt();
            // Tạo predicate cơ bản
            Expression<Func<RegistrationRound, bool>> predicate = x => x.RoundId == roundId;
    
            // Nếu vai trò là REFEREE (trọng tài), chỉ lấy những RegistrationRound mà trọng tài đó chấm điểm
            if (role.ToUpper() == "REFEREE")
            {
                predicate = predicate.AndAlso(x => x.ScoreDetails.Any(sd => sd.RefereeAccountId == GetIdFromJwt()));
            }
            var registrationRounds = await _unitOfWork.GetRepository<RegistrationRound>().GetPagingListAsync(
                predicate: predicate,
                include: query => query.AsSplitQuery()
                    .Include(x => x.RoundResults)
                    .Include(x => x.ScoreDetails)
                    .Include(x => x.Tank)
                    .Include(x => x.Registration)
                        .ThenInclude(x => x.CompetitionCategory)
                    .Include(x => x.Registration)
                        .ThenInclude(x => x.KoiProfile)
                    .Include(x => x.Registration)
                        .ThenInclude(x => x.KoiShow)
                    .Include(x => x.Registration)
                        .ThenInclude(x => x.KoiMedia)
                    .Include(x => x.RoundResults),
                orderBy: q => q.OrderByDescending(x => x.RoundResults.FirstOrDefault().TotalScore),
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
        
        public async Task<CheckQrRegistrationRoundResponse> GetRegistrationRoundByIdAndRoundAsync(Guid registrationId, Guid roundId)
        {
            var registrationRound = await _unitOfWork.GetRepository<RegistrationRound>().SingleOrDefaultAsync(
                predicate: r => r.RegistrationId == registrationId && r.RoundId == roundId,
                include: query => query
                    .Include(r => r.Registration)
                        .ThenInclude(r => r.KoiProfile)
                            .ThenInclude(r => r.Variety)
                    .Include(r => r.Registration)
                        .ThenInclude(r => r.KoiMedia));
            if (registrationRound == null)
            {
                throw new NotFoundException("Registration round not found.");
            }

            return registrationRound.Adapt<CheckQrRegistrationRoundResponse>();
        }
        
    }
}
