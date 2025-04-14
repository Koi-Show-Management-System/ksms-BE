using KSMS.Application.GoogleServices;
using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.TicketOrder;
using KSMS.Domain.Dtos.Responses.Ticket;
using KSMS.Domain.Dtos.Responses.TicketOrder;
using KSMS.Domain.Entities;
using KSMS.Domain.Enums;
using KSMS.Domain.Exceptions;
using KSMS.Infrastructure.Database;
using KSMS.Infrastructure.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Net.payOS;
using Net.payOS.Types;
using System.Linq.Expressions;
using System.Security.Claims;
using Hangfire;
using KSMS.Domain.Common;
using KSMS.Domain.Pagination;
using Mapster;

namespace KSMS.Infrastructure.Services;

public class TicketOrderService : BaseService<TicketOrder>, ITicketOrderService
{
    private readonly PayOS _payOs;
    private readonly IFirebaseService _firebaseService;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IEmailService _emailService;
    public TicketOrderService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<TicketOrder> logger, IHttpContextAccessor httpContextAccessor, PayOS payOs, IFirebaseService firebaseService, IBackgroundJobClient backgroundJobClient, IEmailService emailService) : base(unitOfWork, logger, httpContextAccessor)
    {
        _payOs = payOs;
        _firebaseService = firebaseService;
        _backgroundJobClient = backgroundJobClient;
        _emailService = emailService;
    }

    public async Task<CheckOutTicketResponse> CreateTicketOrder(CreateTicketOrderRequest createTicketOrderRequest)
    {
        var accountId = GetIdFromJwt();
        var totalAmount = 0.0m;
        var timestamp = DateTimeOffset.Now.ToString("yyMMddHHmmss");
        var random = new Random().Next(1000, 9999).ToString(); //
        var transactionCode = long.Parse($"{timestamp}{random}");
        
        // Lấy thông tin về loại vé đầu tiên để xác định show
        if (createTicketOrderRequest.ListOrder == null || !createTicketOrderRequest.ListOrder.Any())
        {
            throw new BadRequestException("Danh sách vé không được để trống");
        }
        
        var firstTicketTypeId = createTicketOrderRequest.ListOrder.First().TicketTypeId;
        var firstTicketType = await _unitOfWork.GetRepository<TicketType>()
            .SingleOrDefaultAsync(
                predicate: p => p.Id == firstTicketTypeId,
                include: query => query.Include(t => t.KoiShow)
                    .ThenInclude(k => k.ShowStatuses)
            );
            
        if (firstTicketType == null)
        {
            throw new NotFoundException($"Không tìm thấy loại vé có ID {firstTicketTypeId}");
        }
        
        // Kiểm tra xem show có bị hủy không
        if (firstTicketType.KoiShow.Status == Domain.Enums.ShowStatus.Cancelled.ToString().ToLower())
        {
            throw new BadRequestException("Triển lãm đã bị hủy. Không thể mua vé.");
        }
        
        // Kiểm tra thời gian cho phép mua vé
        var currentTime = VietNamTimeUtil.GetVietnamTime();
        var ticketCheckInStatus = firstTicketType.KoiShow.ShowStatuses
            .FirstOrDefault(s => s.StatusName == ShowProgress.TicketCheckIn.ToString());
            
        if (ticketCheckInStatus != null && currentTime >= ticketCheckInStatus.StartDate)
        {
            throw new BadRequestException("Đã quá thời gian cho phép mua vé. Triển lãm đã bắt đầu giai đoạn check-in vé.");
        }
        
        // Kiểm tra các loại vé và tính tổng tiền
        foreach (var ticketType in createTicketOrderRequest.ListOrder)
        {
            var ticketTypeDb = await _unitOfWork.GetRepository<TicketType>()
                .SingleOrDefaultAsync(predicate: p => p.Id == ticketType.TicketTypeId);
            if (ticketTypeDb == null)
            {
                throw new NotFoundException($"Không tìm thấy loại vé có ID {ticketType.TicketTypeId}");
            }
            
            // Kiểm tra xem tất cả các vé có thuộc cùng một show không
            if (ticketTypeDb.KoiShowId != firstTicketType.KoiShowId)
            {
                throw new BadRequestException("Không thể mua vé của nhiều triển lãm khác nhau trong cùng một đơn hàng");
            }

            if (ticketType.Quantity > ticketTypeDb.AvailableQuantity)
            {
                throw new BadRequestException($"Loại vé '{ticketTypeDb.Name}' không đủ số lượng");
            }
            totalAmount += ticketTypeDb.Price * ticketType.Quantity;
        }

        var ticketOrder = new TicketOrder()
        {
            FullName = createTicketOrderRequest.FullName,
            Email = createTicketOrderRequest.Email,
            AccountId = accountId,
            OrderDate = VietNamTimeUtil.GetVietnamTime(),
            TransactionCode = transactionCode.ToString(),
            TotalAmount = totalAmount,
            PaymentMethod = PaymentMethod.PayOs.ToString(),
            Status = OrderStatus.Pending.ToString().ToLower(),
        };
        await _unitOfWork.GetRepository<TicketOrder>().InsertAsync(ticketOrder);
        await _unitOfWork.CommitAsync();
        var ticketOrderDetails = new List<TicketOrderDetail>();
        foreach (var x in createTicketOrderRequest.ListOrder)
        {
            var ticketOrderDetail = new TicketOrderDetail
            {
                TicketOrderId = ticketOrder.Id,
                TicketTypeId = x.TicketTypeId,
                Quantity = x.Quantity,
                UnitPrice = (await _unitOfWork.GetRepository<TicketType>()
                    .SingleOrDefaultAsync(predicate: t => t.Id == x.TicketTypeId)).Price
            };
            ticketOrderDetails.Add(ticketOrderDetail);
        }

        await _unitOfWork.GetRepository<TicketOrderDetail>().InsertRangeAsync(ticketOrderDetails);
        await _unitOfWork.CommitAsync();
        var items = new List<ItemData>();
        var ticketOrderDetailDbs = await _unitOfWork.GetRepository<TicketOrderDetail>()
            .GetListAsync(predicate: t => t.TicketOrderId == ticketOrder.Id,
                include: query => query.Include(t => t.TicketType));
        foreach (var x in ticketOrderDetailDbs)
        {
            var item = new ItemData(x.TicketType.Name, x.Quantity, (int)x.UnitPrice);
            items.Add(item);
        }
        var baseUrl = $"{AppConfig.AppSetting.BaseUrl}/api/v1/ticket-order" + "/call-back";
        var url = $"{baseUrl}?ticketOrderId={ticketOrder.Id}";
        var paymentData = new PaymentData(transactionCode, (int)(ticketOrder.TotalAmount), "Buy Ticket", items
            , url, url);
        var createPayment = await _payOs.createPaymentLink(paymentData);
        return new CheckOutTicketResponse
        {
            Message = "Thanh toán thành công",
            Url = createPayment.checkoutUrl
        };
    }

    public async Task UpdateTicketOrder(Guid ticketOrderId, OrderStatus orderStatus)
    {
        var order = await _unitOfWork.GetRepository<TicketOrder>().SingleOrDefaultAsync(
            predicate: x => x.Id == ticketOrderId,
            include: query => query
                .Include(x => x.TicketOrderDetails)
                    .ThenInclude(x => x.TicketType)
        );

        if (order == null)
        {
            throw new NotFoundException("Không tìm thấy đơn hàng");
        }

        order.Status = orderStatus switch
        {
            OrderStatus.Cancelled => OrderStatus.Cancelled.ToString().ToLower(),
            OrderStatus.Paid => OrderStatus.Paid.ToString().ToLower(),
            _ => order.Status
        };

        if (order.Status == OrderStatus.Paid.ToString().ToLower())
        {
            var koiShow = await _unitOfWork.GetRepository<KoiShow>()
                .SingleOrDefaultAsync(predicate: x => x.Id == order.TicketOrderDetails.First().TicketType.KoiShowId);
            var ticketTypes = new List<TicketType>();
            var tickets = new List<Ticket>();
            var qrCodeUploadTasks = new List<Task<string>>();
            foreach (var ticketOrderDetail in order.TicketOrderDetails)
            {
                var ticketType = ticketOrderDetail.TicketType;
                ticketType.AvailableQuantity -= ticketOrderDetail.Quantity;
                ticketTypes.Add(ticketType);

                for (int i = 0; i < ticketOrderDetail.Quantity; i++)
                {
                    var ticketId = Guid.NewGuid();
                    var ticket = new Ticket
                    {
                        Id = ticketId,
                        TicketOrderDetailId = ticketOrderDetail.Id,
                        Status = TicketStatus.Sold.ToString().ToLower(),
                        ExpiredDate = koiShow.EndDate ?? DateTime.Now,
                    };
                    tickets.Add(ticket);
                    var qrCodeTask = _firebaseService.UploadImageAsync(
                        FileUtils.ConvertBase64ToFile(
                            QrcodeUtil.GenerateQrCode(ticketId)
                        ),
                        "ticketQrCodes/"
                    );
                    qrCodeUploadTasks.Add(qrCodeTask);
                }
            }
            var qrCodeUrls = await Task.WhenAll(qrCodeUploadTasks);
            for (int i = 0; i < tickets.Count; i++)
            {
                tickets[i].QrcodeData = qrCodeUrls[i];
            }
            _unitOfWork.GetRepository<TicketType>().UpdateRange(ticketTypes);
            await _unitOfWork.GetRepository<Ticket>().InsertRangeAsync(tickets);
            _unitOfWork.GetRepository<TicketOrder>().UpdateAsync(order);
            await _unitOfWork.CommitAsync();
            _backgroundJobClient.Enqueue(() => _emailService.SendConfirmationTicket(ticketOrderId));
        }
        else
        {
            _unitOfWork.GetRepository<TicketOrder>().UpdateAsync(order);
            await _unitOfWork.CommitAsync();
        }
    }

    public async Task<Paginate<GetAllOrderResponse>> GetAllOrder(Guid? koiShowId, OrderStatus? orderStatus, int page, int size)
    {
        var accountId = GetIdFromJwt();
        var claims = _httpContextAccessor.HttpContext?.User;
        var role = claims?.FindFirst(ClaimTypes.Role)?.Value;
        
        Expression<Func<TicketOrder, bool>> predicate = role?.ToUpper() switch
        {
            "ADMIN" => x => true,
            
            "MANAGER" or "STAFF" => x => x.TicketOrderDetails
                .Any(d => d.TicketType.KoiShow.ShowStaffs
                    .Any(s => s.AccountId == accountId)),
            
            "REFEREE" => x => false,
            
            _ => x => x.AccountId == accountId
        };
        if (koiShowId.HasValue)
        {
            var koiShowPredicate = PredicateBuilder.New<TicketOrder>(x => 
                x.TicketOrderDetails.Any(d => d.TicketType.KoiShowId == koiShowId));
            predicate = predicate.And(koiShowPredicate);
        }
        if (orderStatus.HasValue)
        {
            var statusString = orderStatus.Value.ToString().ToLower();
            var statusPredicate = PredicateBuilder.New<TicketOrder>(x => 
                x.Status == statusString);
            predicate = predicate.And(statusPredicate);
        }
        var orders = await _unitOfWork.GetRepository<TicketOrder>()
            .GetPagingListAsync(
                predicate: predicate,
                orderBy: q => q.OrderByDescending(x => x.OrderDate),
                include: q => q.AsSplitQuery()
                    .Include(x => x.Account)
                    .Include(x => x.TicketOrderDetails)
                        .ThenInclude(x => x.TicketType)
                            .ThenInclude(x => x.KoiShow)
                                .ThenInclude(x => x.ShowStaffs),
                page: page,
                size: size
            );

        // Map sang response
        return orders.Adapt<Paginate<GetAllOrderResponse>>();
        
    }

    public async Task<List<GetOrderDetailResponse>> GetOrderDetailByOrderId(Guid orderId)
    {
        var orderDetails = await _unitOfWork.GetRepository<TicketOrderDetail>()
            .GetListAsync(predicate: x => x.TicketOrderId == orderId,
                include: query => query.Include(x => x.TicketType)
                    .ThenInclude(x => x.KoiShow));
        return orderDetails.Adapt<List<GetOrderDetailResponse>>();
    }

    public async Task<List<GetTicketByOrderDetailResponse>> GetTicketByOrderDetailId(Guid orderDetailId)
    {
        var tickets = await _unitOfWork.GetRepository<Ticket>().GetListAsync(
            predicate: x => x.TicketOrderDetailId == orderDetailId,
            include: query => query.Include(x => x.CheckedInByNavigation));
        return tickets.Adapt<List<GetTicketByOrderDetailResponse>>();
    }

    
}