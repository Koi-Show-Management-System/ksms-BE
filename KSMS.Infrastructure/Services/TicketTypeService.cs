using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.TicketType;
using KSMS.Domain.Entities;
using KSMS.Domain.Exceptions;
using KSMS.Infrastructure.Database;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace KSMS.Infrastructure.Services;

public class TicketTypeService : BaseService<TicketTypeService>, ITicketTypeService
{
    public TicketTypeService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<TicketTypeService> logger, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, httpContextAccessor)
    {
    }

    public async Task CreateTicketTypeAsync(Guid koiShowId, CreateTicketTypeRequest request)
    { 
        var show = await _unitOfWork.GetRepository<KoiShow>().SingleOrDefaultAsync(predicate: x => x.Id == koiShowId);
        if (show == null)
        {
            throw new NotFoundException("Không tìm thấy cuộc thi");
        }
        var ticketType = request.Adapt<TicketType>();
        ticketType.KoiShowId = koiShowId;
        await _unitOfWork.GetRepository<TicketType>().InsertAsync(ticketType);
        await _unitOfWork.CommitAsync();
    }

    public async Task UpdateTicketTypeAsync(Guid id, UpdateTicketTypeRequestV2 request)
    {
        var rule = await _unitOfWork.GetRepository<TicketType>().SingleOrDefaultAsync(predicate: x => x.Id == id);
        if (rule == null)
        {
            throw new NotFoundException("Không tìm thấy loại vé");
        }
        request.Adapt(rule);
        _unitOfWork.GetRepository<TicketType>().UpdateAsync(rule);
        await _unitOfWork.CommitAsync();
    }

    public async Task DeleteTicketTypeAsync(Guid id)
    {
        var rule = await _unitOfWork.GetRepository<TicketType>().SingleOrDefaultAsync(predicate: x => x.Id == id);
        if (rule == null)
        {
            throw new NotFoundException("Không tìm thấy loại vé");
        }
        _unitOfWork.GetRepository<TicketType>().DeleteAsync(rule);
        await _unitOfWork.CommitAsync();
    }
}