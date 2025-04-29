using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.Variety;
using KSMS.Domain.Dtos.Responses.Variety;
using KSMS.Domain.Entities;
using KSMS.Domain.Pagination;
using KSMS.Infrastructure.Database;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using KSMS.Domain.Exceptions;

namespace KSMS.Infrastructure.Services;

public class VarietyService : BaseService<VarietyService>, IVarieryService
{
    public VarietyService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<VarietyService> logger, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, httpContextAccessor)
    {
    }
    public async Task CreateVariety(CreateVarietyRequest createVarietyRequest)
    {
        // Kiểm tra trùng tên (không phân biệt chữ hoa/thường)
        var existingVariety = await _unitOfWork.GetRepository<Variety>()
            .SingleOrDefaultAsync(predicate: v => v.Name.ToLower() == createVarietyRequest.Name.ToLower());
            
        if (existingVariety != null)
        {
            throw new BadRequestException("Tên giống cá đã tồn tại. Vui lòng chọn tên khác");
        }
        
        await _unitOfWork.GetRepository<Variety>().InsertAsync(createVarietyRequest.Adapt<Variety>());
        await _unitOfWork.CommitAsync();
    }

    public async Task UpdateVariety(Guid id, UpdateVarietyRequest updateVarietyRequest)
    {
        var variety = await _unitOfWork.GetRepository<Variety>()
            .SingleOrDefaultAsync(predicate: v => v.Id == id);
        
        if (variety == null)
        {
            throw new NotFoundException("Không tìm thấy giống cá");
        }
        
        // Kiểm tra trùng tên giống cá (loại trừ chính giống cá này)
        var existingVariety = await _unitOfWork.GetRepository<Variety>()
            .SingleOrDefaultAsync(predicate: v => v.Id != id && 
                                               v.Name.ToLower() == updateVarietyRequest.Name.ToLower());
        
        if (existingVariety != null)
        {
            throw new BadRequestException("Tên giống cá đã tồn tại");
        }
        
        updateVarietyRequest.Adapt(variety);
        _unitOfWork.GetRepository<Variety>().UpdateAsync(variety);
        await _unitOfWork.CommitAsync();
    }

    public async Task<Paginate<VarietyResponse>> GetPagingVariety(int page, int size)
    {
        return (await _unitOfWork.GetRepository<Variety>().GetPagingListAsync(page: page, size: size))
            .Adapt<Paginate<VarietyResponse>>();
    }
    
    public async Task DeleteVariety(Guid id)
    {
        var variety = await _unitOfWork.GetRepository<Variety>()
            .SingleOrDefaultAsync(predicate: v => v.Id == id);
            
        if (variety == null)
        {
            throw new NotFoundException("Không tìm thấy giống cá");
        }
        
        // Kiểm tra xem có KoiProfile nào đang sử dụng Variety này không
        var koiProfileCount = await _unitOfWork.GetRepository<KoiProfile>()
            .CountAsync(predicate: kp => kp.VarietyId == id);
            
        if (koiProfileCount > 0)
        {
            throw new BadRequestException($"Không thể xóa giống cá này vì có {koiProfileCount} cá Koi thuộc giống này");
        }
        
        // Kiểm tra xem có CategoryVariety nào đang sử dụng Variety này không
        var categoryVarietyCount = await _unitOfWork.GetRepository<CategoryVariety>()
            .CountAsync(predicate: cv => cv.VarietyId == id);
            
        if (categoryVarietyCount > 0)
        {
            throw new BadRequestException($"Không thể xóa giống cá này vì có {categoryVarietyCount} hạng mục đang sử dụng giống này");
        }
        
        // Nếu không có đối tượng nào sử dụng, tiến hành xóa
        _unitOfWork.GetRepository<Variety>().DeleteAsync(variety);
        await _unitOfWork.CommitAsync();
    }
}