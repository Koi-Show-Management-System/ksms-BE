using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.Sponsor;
using KSMS.Domain.Dtos.Responses.Sponsor;
using KSMS.Domain.Entities;
using KSMS.Domain.Exceptions;
using KSMS.Domain.Pagination;
using KSMS.Infrastructure.Database;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace KSMS.Infrastructure.Services;

public class SponsorService : BaseService<SponsorService>, ISponsorService
{
    public SponsorService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<SponsorService> logger, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, httpContextAccessor)
    {
    }

    public async Task CreateSponsorAsync(Guid koiShowId, CreateSponsorRequest request)
    {
        var show = await _unitOfWork.GetRepository<KoiShow>().SingleOrDefaultAsync(predicate: x => x.Id == koiShowId);
        if (show == null)
        {
            throw new NotFoundException("Không tìm thấy cuộc thi");
        }
        
        // Kiểm tra trùng tên sponsor trong cùng một show
        var existingSponsor = await _unitOfWork.GetRepository<Sponsor>().SingleOrDefaultAsync(
            predicate: s => s.KoiShowId == koiShowId && s.Name.ToLower() == request.Name.ToLower());
            
        if (existingSponsor != null)
        {
            throw new BadRequestException("Tên nhà tài trợ đã tồn tại trong triển lãm này. Vui lòng chọn tên khác");
        }
        
        var sponsor = request.Adapt<Sponsor>();
        sponsor.KoiShowId = koiShowId;
        await _unitOfWork.GetRepository<Sponsor>().InsertAsync(sponsor);
        await _unitOfWork.CommitAsync();
    }

    public async Task UpdateSponsorAsync(Guid id, UpdateSponsorRequestV2 request)
    {
        var sponsor = await _unitOfWork.GetRepository<Sponsor>().SingleOrDefaultAsync(predicate: x => x.Id == id);
        if (sponsor == null)
        {
            throw new NotFoundException("Không tìm thấy nhà tài trợ");
        }
        
        // Kiểm tra trùng tên sponsor trong cùng một show (loại trừ chính sponsor này)
        var existingSponsor = await _unitOfWork.GetRepository<Sponsor>().SingleOrDefaultAsync(
            predicate: s => s.KoiShowId == sponsor.KoiShowId && 
                           s.Id != id && 
                           s.Name.ToLower() == request.Name.ToLower());
            
        if (existingSponsor != null)
        {
            throw new BadRequestException("Tên nhà tài trợ đã tồn tại trong triển lãm này. Vui lòng chọn tên khác");
        }
        
        request.Adapt(sponsor);
        _unitOfWork.GetRepository<Sponsor>().UpdateAsync(sponsor);
        await _unitOfWork.CommitAsync();
    }

    public async Task DeleteSponsorAsync(Guid id)
    {
        var sponsor = await _unitOfWork.GetRepository<Sponsor>().SingleOrDefaultAsync(predicate: x => x.Id == id);
        if (sponsor == null)
        {
            throw new NotFoundException("Không tìm thấy nhà tài trợ");
        }
        _unitOfWork.GetRepository<Sponsor>().DeleteAsync(sponsor);
        await _unitOfWork.CommitAsync();
    }

    public async Task<Paginate<SponsorGetKoiShowDetailResponse>> GetPageSponsorAsync(Guid koiShowId, int page, int size)
    {
        var sponsors = await _unitOfWork.GetRepository<Sponsor>()
            .GetPagingListAsync(predicate: x => x.KoiShowId == koiShowId, page: page, size: size);
        return sponsors.Adapt<Paginate<SponsorGetKoiShowDetailResponse>>();
    }
}