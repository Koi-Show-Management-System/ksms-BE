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
using KSMS.Domain.Pagination;
using Mapster;

namespace KSMS.Infrastructure.Services;

public class TicketOrderService : BaseService<TicketOrder>, ITicketOrderService
{
    private readonly PayOS _payOs;
    private readonly IFirebaseService _firebaseService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public TicketOrderService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<TicketOrder> logger, IHttpContextAccessor httpContextAccessor, PayOS payOs, IFirebaseService firebaseService) : base(unitOfWork, logger, httpContextAccessor)
    {
        _payOs = payOs;
        _firebaseService = firebaseService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<CheckOutTicketResponse> CreateTicketOrder(CreateTicketOrderRequest createTicketOrderRequest)
    {
        var accountId = GetIdFromJwt();
        var totalAmount = 0.0m;
        var timestamp = DateTimeOffset.Now.ToString("yyMMddHHmmss");
        var random = new Random().Next(1000, 9999).ToString(); //
        var transactionCode = long.Parse($"{timestamp}{random}");
        foreach (var ticketType in createTicketOrderRequest.ListOrder)
        {
            var ticketTypeDb = await _unitOfWork.GetRepository<TicketType>()
                .SingleOrDefaultAsync(predicate: p => p.Id == ticketType.TicketTypeId);
            if (ticketTypeDb == null)
            {
                throw new NotFoundException("Ticket Type Id " + $"{ticketType.TicketTypeId} is not found!!");
            }

            if (ticketType.Quantity > ticketTypeDb.AvailableQuantity)
            {
                throw new BadRequestException("Ticket Type: " + $"{ticketTypeDb.Name} is not enough");
            }
            totalAmount += ticketTypeDb.Price * ticketType.Quantity;
        }

        var ticketOrder = new TicketOrder()
        {
            FullName = createTicketOrderRequest.FullName,
            Email = createTicketOrderRequest.Email,
            AccountId = accountId,
            OrderDate = DateTime.UtcNow,
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
        const string baseUrl = "https://localhost:7042/api/ticket-order" + "/call-back";
        var url = $"{baseUrl}?ticketOrderId={ticketOrder.Id}";
        var paymentData = new PaymentData(transactionCode, (int)(ticketOrder.TotalAmount), "Buy Ticket", items
            , url, url);
        var createPayment = await _payOs.createPaymentLink(paymentData);
        return new CheckOutTicketResponse
        {
            Message = "Checkout Successfully",
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
                        .ThenInclude(x => x.KoiShow)
        );

        if (order == null)
        {
            throw new NotFoundException("Order not found");
        }

        order.Status = orderStatus switch
        {
            OrderStatus.Cancelled => OrderStatus.Cancelled.ToString().ToLower(),
            OrderStatus.Paid => OrderStatus.Paid.ToString().ToLower(),
            _ => order.Status
        };

        if (order.Status == OrderStatus.Paid.ToString().ToLower())
        {
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
                        ExpiredDate = new DateTime(9999, 12, 31)
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
            var orderForMail = order;
            foreach (var detail in orderForMail.TicketOrderDetails)
            {
                detail.Tickets = tickets.Where(t => t.TicketOrderDetailId == detail.Id).ToList();
            }

            var sendMail = MailUtil.SendEmail(
                order.Email,
                "KOI SHOW - Xác nhận đơn hàng vé thành công",
                MailUtil.ContentMailUtil.ConfirmTicketOrder(orderForMail),
                ""
            );

            if (!sendMail)
            {
                throw new BadRequestException("Error sending confirmation email");
            }
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
                include: q => q
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