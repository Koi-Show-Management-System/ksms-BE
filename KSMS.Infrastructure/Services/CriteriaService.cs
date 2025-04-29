using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.Criterion;
using KSMS.Domain.Dtos.Responses.Criterion;
using KSMS.Domain.Entities;
using KSMS.Infrastructure.Database;
using Microsoft.Extensions.Logging;
using Mapster;
using KSMS.Domain.Exceptions;
using KSMS.Infrastructure.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KSMS.Domain.Dtos.Responses.CriteriaCompetitionCategory;
using KSMS.Domain.Enums;
using KSMS.Domain.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace KSMS.Infrastructure.Services
{
    public class CriteriaService : BaseService<CriteriaService>, ICriterionService
    {
        public CriteriaService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<CriteriaService> logger, IHttpContextAccessor httpContextAccessor)
          : base(unitOfWork, logger, httpContextAccessor)
        {
        }


        public async Task CreateCriteriaAsync(CreateCriteriaRequest createCriteriaRequest)
        {
            var criterionRepository = _unitOfWork.GetRepository<Criterion>();

            var existingCriterion = await criterionRepository.SingleOrDefaultAsync(c => c.Name == createCriteriaRequest.Name, null, null);
            if (existingCriterion != null)
            {
                throw new BadRequestException($"Tiêu chí có tên '{createCriteriaRequest.Name}' đã tồn tại.");
            }
            await criterionRepository.InsertAsync(createCriteriaRequest.Adapt<Criterion>());
            await _unitOfWork.CommitAsync();

        }


        public async Task<CriteriaResponse> GetCriteriaByIdAsync(Guid id)
        {
            var criterionRepository = _unitOfWork.GetRepository<Criterion>();

            var criterion = await criterionRepository.SingleOrDefaultAsync(
                predicate: c => c.Id == id);

            if (criterion == null)
            {
                throw new NotFoundException("Không tìm thấy tiêu chí");
            }

            return criterion.Adapt<CriteriaResponse>();
        }

        public async Task UpdateCriteriaAsync(Guid id, UpdateCriteriaRequest updateCriteriaRequest)
        {
            var criterionRepository = _unitOfWork.GetRepository<Criterion>();

            var criterion = await criterionRepository.SingleOrDefaultAsync(c => c.Id == id, null, null);
            if (criterion == null)
            {
                throw new NotFoundException("Không tìm thấy tiêu chí");
            }
            updateCriteriaRequest.Adapt(criterion);

            criterionRepository.UpdateAsync(criterion);
            await _unitOfWork.CommitAsync();
        }

        public async Task<Paginate<GetAllCriteriaResponse>> GetPagingCriteria(int page, int size)
        {
            return (await _unitOfWork.GetRepository<Criterion>().GetPagingListAsync(page: page, size: size))
                .Adapt<Paginate<GetAllCriteriaResponse>>();
        }

        public async Task<List<GetCriteriaCompetitionCategoryResponse>> GetCriteriaCompetitionCategory(Guid competitionCategoryId, Guid roundId)
        {
            var round = await _unitOfWork.GetRepository<Round>().SingleOrDefaultAsync(predicate: x => x.Id == roundId);
            var criteriaCompetition = await _unitOfWork.GetRepository<CriteriaCompetitionCategory>()
                .GetListAsync(
                    predicate: x => x.CompetitionCategoryId == competitionCategoryId &&
                                    x.RoundType == round.RoundType,
                    include: query =>
                        query.Include(x => x.Criteria));
            return criteriaCompetition.Adapt<List<GetCriteriaCompetitionCategoryResponse>>();
        }


        public async Task DeleteCriteriaAsync(Guid id)
        {
            var criterionRepository = _unitOfWork.GetRepository<Criterion>();
            var criterion = await criterionRepository.SingleOrDefaultAsync(c => c.Id == id, null, null);

            if (criterion == null)
            {
                throw new NotFoundException("Không tìm thấy tiêu chí");
            }
            
            // Kiểm tra xem có CriteriaCompetitionCategory nào đang sử dụng Criterion này không
            var criteriaCompetitionCount = await _unitOfWork.GetRepository<CriteriaCompetitionCategory>()
                .CountAsync(predicate: cc => cc.CriteriaId == id);
            
            if (criteriaCompetitionCount > 0)
            {
                // Lấy danh sách các hạng mục sử dụng tiêu chí này
                var categories = await _unitOfWork.GetRepository<CriteriaCompetitionCategory>()
                    .GetListAsync(
                        predicate: cc => cc.CriteriaId == id,
                        include: query => query.Include(cc => cc.CompetitionCategory));
                
                // Đếm số lượng hạng mục duy nhất
                var uniqueCategoryCount = categories.Select(cc => cc.CompetitionCategoryId).Distinct().Count();
                
                // Lấy danh sách các vòng thi
                var roundTypes = categories.Select(cc => cc.RoundType).Distinct().ToList();
                
                // Chuyển đổi tên kỹ thuật sang tên hiển thị tiếng Việt
                var translatedRoundTypes = roundTypes.Select(rt => {
                    return rt?.ToLower() switch {
                        "final" => "Vòng chung kết",
                        "preliminary" => "Vòng sơ khảo",
                        "evaluation" => "Vòng đánh giá chính",
                        _ => rt
                    };
                }).ToList();
                
                string roundTypeText = string.Join(", ", translatedRoundTypes.Where(rt => !string.IsNullOrEmpty(rt)));
                
                throw new BadRequestException(
                    $"Không thể xóa tiêu chí này vì đang được sử dụng trong {uniqueCategoryCount} hạng mục thi đấu " +
                    $"(ở {criteriaCompetitionCount} cấu hình đánh giá thuộc các vòng: {roundTypeText})");
            }
            
            // Kiểm tra xem có ErrorType nào đang sử dụng Criterion này không
            var errorTypeCount = await _unitOfWork.GetRepository<ErrorType>()
                .CountAsync(predicate: et => et.CriteriaId == id);
            
            if (errorTypeCount > 0)
            {
                throw new BadRequestException($"Không thể xóa tiêu chí này vì có {errorTypeCount} loại lỗi đang liên kết đến tiêu chí này");
            }
            
            criterionRepository.DeleteAsync(criterion);
            await _unitOfWork.CommitAsync();
        }
    }
}