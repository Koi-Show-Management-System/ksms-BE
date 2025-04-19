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
using Microsoft.EntityFrameworkCore.Storage;

namespace KSMS.Infrastructure.Services;

public class TicketOrderService : BaseService<TicketOrder>, ITicketOrderService
{
    private readonly PayOS _payOs;
    private readonly IFirebaseService _firebaseService;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;
    public TicketOrderService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<TicketOrder> logger, IHttpContextAccessor httpContextAccessor, PayOS payOs, IFirebaseService firebaseService, IBackgroundJobClient backgroundJobClient, IEmailService emailService, INotificationService notificationService) : base(unitOfWork, logger, httpContextAccessor)
    {
        _payOs = payOs;
        _firebaseService = firebaseService;
        _backgroundJobClient = backgroundJobClient;
        _emailService = emailService;
        _notificationService = notificationService;
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
        
        // Sử dụng transaction để đảm bảo tính nhất quán khi kiểm tra và cập nhật số lượng vé
        using (var transaction = await _unitOfWork.BeginTransactionAsync())
        {
            try
            {
                // Kiểm tra các loại vé và tính tổng tiền
                var ticketTypeInfos = new List<(TicketType Type, int Quantity)>();
                
                foreach (var ticketType in createTicketOrderRequest.ListOrder)
                {
                    // Lấy thông tin loại vé mà không cần tracking
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
                    
                    // Lưu lại thông tin để cập nhật sau
                    ticketTypeInfos.Add((ticketTypeDb, ticketType.Quantity));
                    
                    totalAmount += ticketTypeDb.Price * ticketType.Quantity;
                }
                
                // Cập nhật số lượng vé trong database bằng repository
                foreach (var info in ticketTypeInfos)
                {
                    // Sau khi kiểm tra, cập nhật và gọi UpdateAsync
                    info.Type.AvailableQuantity -= info.Quantity;
                    _unitOfWork.GetRepository<TicketType>().UpdateAsync(info.Type);
                }
                
                var orderDate = VietNamTimeUtil.GetVietnamTime();
                var ticketOrder = new TicketOrder()
                {
                    Id = Guid.NewGuid(),
                    FullName = createTicketOrderRequest.FullName,
                    Email = createTicketOrderRequest.Email,
                    AccountId = accountId,
                    OrderDate = orderDate,
                    TransactionCode = transactionCode.ToString(),
                    TotalAmount = totalAmount,
                    PaymentMethod = PaymentMethod.PayOs.ToString(),
                    Status = OrderStatus.Pending.ToString().ToLower(),
                };
                await _unitOfWork.GetRepository<TicketOrder>().InsertAsync(ticketOrder);
                
                var ticketOrderDetails = new List<TicketOrderDetail>();
                foreach (var info in ticketTypeInfos)
                {
                    var ticketOrderDetail = new TicketOrderDetail
                    {
                        TicketOrderId = ticketOrder.Id,
                        TicketTypeId = info.Type.Id,
                        Quantity = info.Quantity,
                        UnitPrice = info.Type.Price
                    };
                    ticketOrderDetails.Add(ticketOrderDetail);
                }

                await _unitOfWork.GetRepository<TicketOrderDetail>().InsertRangeAsync(ticketOrderDetails);
                await _unitOfWork.CommitAsync();
                
                // Đặt lịch kiểm tra hết hạn thanh toán (11 phút sau khi tạo đơn)
                _backgroundJobClient.Schedule(
                    () => HandleExpiredOrder(ticketOrder.Id),
                    TimeSpan.FromMinutes(3) 
                );
                
                await transaction.CommitAsync();
                
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
                
                // Tính thời gian hết hạn dựa trên thiết lập OrderDate + 2 phút
                var expiryTime = orderDate.AddMinutes(2);

                // Chuyển đổi sang Unix timestamp đảm bảo sử dụng múi giờ Việt Nam (UTC+7)
                // Chỉ định rõ múi giờ UTC+7 khi tạo DateTimeOffset để tránh sai lệch
                var expiredAtTimestamp = new DateTimeOffset(
                    expiryTime,
                    new TimeSpan(7, 0, 0) // Chỉ định múi giờ Việt Nam (UTC+7)
                ).ToUnixTimeSeconds();
                
                _logger.LogInformation($"Order: {ticketOrder.Id}, OrderDate: {orderDate}, ExpiryTime: {expiryTime}, Timestamp: {expiredAtTimestamp}");
                
                // Sử dụng mô tả ngắn gọ
                
                var paymentData = new PaymentData(
                    transactionCode, 
                    (int)(ticketOrder.TotalAmount), 
                    "Mua vé", 
                    items, 
                    url, 
                    url,
                    expiredAt: expiredAtTimestamp
                );
                var createPayment = await _payOs.createPaymentLink(paymentData);
                
                // Lưu URL thanh toán vào đơn hàng
                ticketOrder.PaymentUrl = createPayment.checkoutUrl;
                _unitOfWork.GetRepository<TicketOrder>().UpdateAsync(ticketOrder);
                await _unitOfWork.CommitAsync();
                
                return new CheckOutTicketResponse
                {
                    Message = "Thanh toán thành công",
                    Url = createPayment.checkoutUrl
                };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

    public async Task UpdateTicketOrder(Guid ticketOrderId, OrderStatus orderStatus)
    {
        using (var transaction = await _unitOfWork.BeginTransactionAsync())
        {
            try
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

                // Kiểm tra nếu đơn hàng đã hết hạn (OrderDate + 2 phút)
                var expiryTime = order.OrderDate.AddMinutes(2);
                var currentTime = VietNamTimeUtil.GetVietnamTime();
                
                // Nếu đơn hàng đã hết hạn nhưng chưa được xử lý bởi Hangfire
                if (order.Status == OrderStatus.Pending.ToString().ToLower() && 
                    expiryTime <= currentTime)
                {
                    // Xử lý đơn hàng hết hạn ngay lập tức thay vì chờ Hangfire
                    await HandleExpiredOrderImmediate(order, transaction);
                    throw new BadRequestException("Đơn hàng đã hết hạn thanh toán");
                }

                // Kiểm tra nếu đơn hàng đã được xử lý trước đó
                if (order.Status == OrderStatus.Paid.ToString().ToLower() || 
                    order.Status == OrderStatus.Cancelled.ToString().ToLower() ||
                    order.Status == OrderStatus.Expired.ToString().ToLower())
                {
                    throw new BadRequestException("Đơn hàng đã được xử lý trước đó");
                }

                // Cập nhật trạng thái đơn hàng
                var newStatus = orderStatus switch
                {
                    OrderStatus.Cancelled => OrderStatus.Cancelled.ToString().ToLower(),
                    OrderStatus.Paid => OrderStatus.Paid.ToString().ToLower(),
                    _ => order.Status
                };

                // Cập nhật trạng thái đơn hàng
                order.Status = newStatus;
                _unitOfWork.GetRepository<TicketOrder>().UpdateAsync(order);

                if (newStatus == OrderStatus.Paid.ToString().ToLower())
                {
                    var koiShow = await _unitOfWork.GetRepository<KoiShow>()
                        .SingleOrDefaultAsync(predicate: x => x.Id == order.TicketOrderDetails.First().TicketType.KoiShowId);
                    var tickets = new List<Ticket>();
                    var qrCodeUploadTasks = new List<Task<string>>();
                    
                    foreach (var ticketOrderDetail in order.TicketOrderDetails)
                    {
                        // Không cần giảm AvailableQuantity vì đã giảm khi tạo đơn hàng
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
                    
                    await _unitOfWork.GetRepository<Ticket>().InsertRangeAsync(tickets);
                    await _unitOfWork.CommitAsync();
                    _backgroundJobClient.Enqueue(() => _emailService.SendConfirmationTicket(ticketOrderId));
                }
                else if (newStatus == OrderStatus.Cancelled.ToString().ToLower())
                {
                    // Hoàn trả số lượng vé khi hủy đơn hàng - lấy phiên bản mới nhất từ database
                    foreach (var detail in order.TicketOrderDetails)
                    {
                        // Lấy TicketTypeId từ detail thay vì dùng navigation property
                        var ticketTypeId = detail.TicketTypeId;
                        var quantity = detail.Quantity;
                        
                        // Lấy phiên bản mới nhất của TicketType từ database
                        var ticketType = await _unitOfWork.GetRepository<TicketType>()
                            .SingleOrDefaultAsync(predicate: t => t.Id == ticketTypeId);
                            
                        if (ticketType != null)
                        {
                            // Cập nhật số lượng
                            ticketType.AvailableQuantity += quantity;
                            
                            // Cập nhật vào database
                            _unitOfWork.GetRepository<TicketType>().UpdateAsync(ticketType);
                        }
                    }
                    
                    await _unitOfWork.CommitAsync();
                }
                else
                {
                    await _unitOfWork.CommitAsync();
                }
                
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                
                // Kiểm tra xem exception có phải là BadRequestException không
                if (ex is BadRequestException)
                {
                    throw; // Ném lại exception nếu là BadRequestException
                }
                
                // Nếu là loại exception khác, log và ném một exception chung
                _logger.LogError(ex, "Lỗi khi cập nhật đơn hàng");
                throw new Exception("Đã xảy ra lỗi khi xử lý đơn hàng. Vui lòng thử lại sau.", ex);
            }
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

    // Phương thức xử lý đơn hàng hết hạn
    public async Task HandleExpiredOrder(Guid orderId)
    {
        // Lấy thông tin đơn hàng với order details
        var order = await _unitOfWork.GetRepository<TicketOrder>().SingleOrDefaultAsync(
            predicate: x => x.Id == orderId,
            include: query => query
                .Include(x => x.TicketOrderDetails)
        );

        if (order == null)
        {
            _logger.LogWarning($"Không tìm thấy đơn hàng: {orderId}");
            return;
        }

        // Tính thời gian hết hạn: OrderDate + 2 phút
        var expiryTime = order.OrderDate.AddMinutes(2);
        var currentTime = VietNamTimeUtil.GetVietnamTime();
        
        // Chỉ xử lý các đơn hàng còn đang pending và đã quá hạn
        if (order.Status == OrderStatus.Pending.ToString().ToLower() && 
            expiryTime <= currentTime)
        {
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    // Lưu thông tin cần thiết của đơn hàng
                    var accountId = order.AccountId;
                    var transactionCode = order.TransactionCode;
                    var totalAmount = order.TotalAmount;
                    
                    // Tìm thông tin show từ orderId
                    string showName = "Không xác định";
                    var firstTicketOrderDetail = await _unitOfWork.GetRepository<TicketOrderDetail>()
                        .SingleOrDefaultAsync(
                            predicate: od => od.TicketOrderId == orderId,
                            include: query => query.Include(od => od.TicketType)
                                .ThenInclude(tt => tt.KoiShow)
                        );
                        
                    if (firstTicketOrderDetail?.TicketType?.KoiShow != null)
                    {
                        showName = firstTicketOrderDetail.TicketType.KoiShow.Name;
                    }
                    
                    // Xử lý đơn hàng hết hạn
                    await HandleExpiredOrderImmediate(order, transaction);
                    
                    // Gửi thông báo cho người dùng sau khi xử lý thành công
                    if (accountId != null)
                    {
                        await _notificationService.SendNotification(
                            accountId,
                            "Đơn hàng vé đã hết hạn",
                            $"Đơn hàng vé tham quan triển lãm {showName} với mã giao dịch {transactionCode} trị giá {totalAmount:N0} VNĐ đã bị hủy do quá thời gian thanh toán. Bạn có thể tạo đơn hàng mới nếu muốn.",
                            NotificationType.Payment
                        );
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, $"Lỗi khi xử lý đơn hàng hết hạn: {orderId}");
                }
            }
        }
    }

    // Phương thức xử lý nghiệp vụ đơn hàng hết hạn
    private async Task HandleExpiredOrderImmediate(TicketOrder order, IDbContextTransaction transaction)
    {
        try
        {
            // Cập nhật trạng thái đơn hàng thành "cancelled"
            order.Status = OrderStatus.Cancelled.ToString().ToLower();
            _unitOfWork.GetRepository<TicketOrder>().UpdateAsync(order);

            // Hoàn trả số lượng vé - Lấy dữ liệu mới nhất từ database
            foreach (var detail in order.TicketOrderDetails)
            {
                // Lấy TicketTypeId từ detail thay vì dùng navigation property
                var ticketTypeId = detail.TicketTypeId;
                var quantity = detail.Quantity;
                
                // Lấy phiên bản mới nhất của TicketType từ database
                var ticketType = await _unitOfWork.GetRepository<TicketType>()
                    .SingleOrDefaultAsync(predicate: t => t.Id == ticketTypeId);
                    
                if (ticketType != null)
                {
                    // Cập nhật số lượng
                    ticketType.AvailableQuantity += quantity;
                    
                    // Cập nhật vào database
                    _unitOfWork.GetRepository<TicketType>().UpdateAsync(ticketType);
                }
            }

            await _unitOfWork.CommitAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Lỗi khi xử lý hoàn trả vé cho đơn hàng {order.Id}");
            throw; // Ném lại exception để caller có thể xử lý rollback
        }
    }
}