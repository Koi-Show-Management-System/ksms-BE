using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Responses.Ticket;
using KSMS.Domain.Entities;
using KSMS.Domain.Enums;
using KSMS.Domain.Exceptions;
using KSMS.Infrastructure.Database;
using KSMS.Infrastructure.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KSMS.Infrastructure.Services;

public class TicketService : BaseService<TicketService>, ITicketService
{
    private readonly INotificationService _notificationService;
    public TicketService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<TicketService> logger, IHttpContextAccessor httpContextAccessor, INotificationService notificationService) : base(unitOfWork, logger, httpContextAccessor)
    {
        _notificationService = notificationService;
    }

    public async Task<GetTicketInfoByQrCode> GetTicketInfoByQrCode(Guid id)
    {
        var ticket = await _unitOfWork.GetRepository<Ticket>()
            .SingleOrDefaultAsync(
                predicate: t => t.Id == id,
                include: query => query
                    .Include(t => t.TicketOrderDetail)
                        .ThenInclude(tod => tod.TicketOrder)
                    .ThenInclude(t => t.Account)
                    .Include(t => t.TicketOrderDetail)
                        .ThenInclude(tod => tod.TicketType)
                            .ThenInclude(tt => tt.KoiShow));
        if (ticket is null)
        {
            throw new NotFoundException("Không tìm thấy vé với mã QR này");
        }

        if (ticket.Status == TicketStatus.Checkin.ToString().ToLower())
        {
            throw new BadRequestException("Vé đã được checkin trước đó");
        }
        if (ticket.Status == TicketStatus.Cancelled.ToString().ToLower())
        {
            throw new BadRequestException("Vé đã bị hủy");
        }
        if (ticket.TicketOrderDetail.TicketType.KoiShow.EndDate < VietNamTimeUtil.GetVietnamTime())
        {
            throw new BadRequestException("Triển lãm đã kết thúc");
        }

        return new GetTicketInfoByQrCode
        {
            Id = ticket.Id,
            Status = ticket.Status,
            CheckInTime = ticket.CheckInTime,
            CheckInLocation = ticket.CheckInLocation,
            TicketTypeName = ticket.TicketOrderDetail.TicketType.Name,
            TicketPrice = ticket.TicketOrderDetail.TicketType.Price,
            FullName = ticket.TicketOrderDetail.TicketOrder.FullName,
            Email = ticket.TicketOrderDetail.TicketOrder.Email,
            Phone = ticket.TicketOrderDetail.TicketOrder.Account.Phone,
            ShowName = ticket.TicketOrderDetail.TicketType.KoiShow.Name,
            ShowStartDate = ticket.TicketOrderDetail.TicketType.KoiShow.StartDate,
            ShowEndDate = ticket.TicketOrderDetail.TicketType.KoiShow.EndDate,
            ShowLocation = ticket.TicketOrderDetail.TicketType.KoiShow.Location
        };
    }

    public async Task RefundTicket(Guid ticketOrderId)
    {
        var ticketOrder = await _unitOfWork.GetRepository<TicketOrder>()
            .SingleOrDefaultAsync(
                predicate: to => to.Id == ticketOrderId,
                include: query => query.Include(to => to.Account));
        if (ticketOrder is null)
        {
            throw new NotFoundException($"Không tìm thấy đơn hàng vé với ID {ticketOrderId}");
        }

        var ticketOrderDetails = await _unitOfWork.GetRepository<TicketOrderDetail>()
            .GetListAsync(
                predicate: tod => tod.TicketOrderId == ticketOrderId,
                include: query => query
                    .Include(tod => tod.TicketType)
                    .ThenInclude(tt => tt.KoiShow));
        if (!ticketOrderDetails.Any())
        {
            throw new NotFoundException("Không tìm thấy chi tiết đơn hàng");
        }
        var showName = ticketOrderDetails.FirstOrDefault()?.TicketType.KoiShow.Name;
        var allTickets = new List<Ticket>();
        foreach (var detail in ticketOrderDetails)
        {
            var tickets = await _unitOfWork.GetRepository<Ticket>()
                .GetListAsync(predicate: t => t.TicketOrderDetailId == detail.Id);
            foreach (var ticket in tickets)
            {
                ticket.Status = TicketStatus.Refunded.ToString().ToLower();
            }
            allTickets.AddRange(tickets);
        }
        if (allTickets.Any())
        {
            _unitOfWork.GetRepository<Ticket>().UpdateRange(allTickets);
            await _unitOfWork.CommitAsync();
        }
        await _notificationService.SendNotification(
            ticketOrder.AccountId,
            $"Đơn hàng vé với mã giao dịch {ticketOrder.TransactionCode} đã được hoàn tiền",
            $"Đơn hàng vé của bạn cho triển lãm {showName} đã được hoàn tiền do triển lãm bị hủy. Vui lòng kiểm tra lại tài khoản của bạn trong 3-5 ngày làm việc",
            NotificationType.System);
    }

    public async Task CheckInTicket(Guid id)
    {
        var ticket = await _unitOfWork.GetRepository<Ticket>()
            .SingleOrDefaultAsync(predicate: t => t.Id == id);
        if (ticket is null)
        {
            throw new NotFoundException("Không tìm thấy vé");
        }
        var ticketOrderDetail = await _unitOfWork.GetRepository<TicketOrderDetail>()
            .SingleOrDefaultAsync(predicate: tod => tod.Id == ticket.TicketOrderDetailId,
                include: query => query
                    .Include(tod => tod.TicketType)
                        .ThenInclude(tt => tt.KoiShow)
                    .Include(t => t.TicketOrder)
                    .ThenInclude(t => t.Account));
        if (ticket.Status == TicketStatus.Checkin.ToString().ToLower())
        {
            throw new BadRequestException("Vé đã được checkin trước đó");
        }
        if (ticket.Status == TicketStatus.Cancelled.ToString().ToLower())
        {
            throw new BadRequestException("Vé đã bị hủy");
        }
        if (ticket.TicketOrderDetail.TicketType.KoiShow.EndDate < VietNamTimeUtil.GetVietnamTime())
        {
            throw new BadRequestException("Triển lãm đã kết thúc");
        }
        ticket.Status = TicketStatus.Checkin.ToString().ToLower();
        ticket.CheckInTime = VietNamTimeUtil.GetVietnamTime();
        ticket.CheckInLocation = ticketOrderDetail.TicketType.KoiShow.Location;
        ticket.CheckedInBy = GetIdFromJwt();
        _unitOfWork.GetRepository<Ticket>().UpdateAsync(ticket);
        await _unitOfWork.CommitAsync();
        await _notificationService.SendNotification(
            ticketOrderDetail.TicketOrder.AccountId,
            $"Vé {ticketOrderDetail.TicketType.Name} đã được checkin",
            $"Vé {ticketOrderDetail.TicketType.Name} đã được checkin tại {ticketOrderDetail.TicketType.KoiShow.Location}",
            NotificationType.System);
        
    }
}