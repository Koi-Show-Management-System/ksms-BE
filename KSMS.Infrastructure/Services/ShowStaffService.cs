using System.Linq.Expressions;
using KSMS.Application.Extensions;
using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Responses.ShowStaff;
using KSMS.Domain.Entities;
using KSMS.Domain.Enums;
using KSMS.Domain.Exceptions;
using KSMS.Domain.Pagination;
using KSMS.Infrastructure.Database;
using KSMS.Infrastructure.Utils;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KSMS.Infrastructure.Services;

public class ShowStaffService : BaseService<ShowStaffService>, IShowStaffService
{
    public ShowStaffService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<ShowStaffService> logger, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, httpContextAccessor)
    {
    }

    public async Task AddStaffOrManager(Guid showId, Guid accountId)
    {
        var show = await _unitOfWork.GetRepository<KoiShow>().SingleOrDefaultAsync(predicate: x => x.Id == showId);
        if (show == null)
        {
            throw new NotFoundException("Không tìm thấy cuộc thi");
        }

        var account = await _unitOfWork.GetRepository<Account>()
            .SingleOrDefaultAsync(predicate: x => x.Id == accountId);
        if (account == null)
        {
            throw new NotFoundException("Không tìm thấy tài khoản");
        }

        var showStaff = await _unitOfWork.GetRepository<ShowStaff>()
            .SingleOrDefaultAsync(predicate: x => x.KoiShowId == showId && x.AccountId == accountId);
        if (showStaff != null)
        {
            throw new BadRequestException("Tài khoản này đã được phân công cho cuộc thi");
        }
        if (account.Role != RoleName.Staff.ToString() && account.Role != RoleName.Manager.ToString())
        {
            throw new BadRequestException("Tài khoản không phải là nhân viên hoặc quản lý");
        }
        if (GetRoleFromJwt() == RoleName.Manager.ToString())
        {
            if (account.Role == RoleName.Manager.ToString())
            {
                throw new BadRequestException("Quản lý không thể phân công quản lý khác");
            }
        }
        await _unitOfWork.GetRepository<ShowStaff>().InsertAsync(new ShowStaff
        {
            KoiShowId = show.Id,
            AccountId = accountId,
            AssignedBy = GetIdFromJwt(),
            AssignedAt = VietNamTimeUtil.GetVietnamTime(),
        });
        await _unitOfWork.CommitAsync();
    }

    public async Task RemoveStaffOrManager(Guid id)
    {
        var showStaff = await _unitOfWork.GetRepository<ShowStaff>().SingleOrDefaultAsync(predicate: x => x.Id == id,
            include: x => x.Include(y => y.Account));
        if (showStaff == null)
        {
            throw new NotFoundException("Không tìm thấy quản lý hoặc nhân viên");
        }
        if (GetRoleFromJwt() == RoleName.Manager.ToString())
        {
            if (showStaff.Account.Role == RoleName.Manager.ToString())
            {
                throw new BadRequestException("Quản lý không thể xóa quản lý khác");
            }
        }
        _unitOfWork.GetRepository<ShowStaff>().DeleteAsync(showStaff);
        await _unitOfWork.CommitAsync();
    }

    public async Task<Paginate<GetPageStaffAndManagerResponse>> GetPageStaffAndManager(Guid showId, RoleName? role, int page, int size)
    {
        var show = await _unitOfWork.GetRepository<KoiShow>().SingleOrDefaultAsync(predicate: x => x.Id == showId);
        if (show == null)
        {
            throw new NotFoundException("Không tìm thấy cuộc thi");
        }
        Expression<Func<ShowStaff, bool>> filterQuery = account => account.KoiShowId == showId;
        if (role.HasValue)
        {
            var roleString = role.Value.ToString();
            filterQuery = filterQuery.AndAlso(r => r.Account.Role == roleString);
        }
        var showStaffs = await _unitOfWork.GetRepository<ShowStaff>().GetPagingListAsync(
            predicate: filterQuery,
            include: x => x.Include(y => y.Account)
                .Include(y => y.AssignedByNavigation), 
            page: page,
            size: size);
        return showStaffs.Adapt<Paginate<GetPageStaffAndManagerResponse>>();
    }
}