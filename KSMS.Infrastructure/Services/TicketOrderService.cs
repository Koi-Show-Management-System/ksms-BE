﻿using KSMS.Application.GoogleServices;
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
        var timestamp = DateTimeOffset.Now.ToString("yyMMddHHmmss");
        var random = new Random().Next(1000, 9999).ToString(); //
        var transactionCode = long.Parse($"{timestamp}{random}");
        
        // Lấy thông tin về loại vé đầu tiên để xác định show
        if (createTicketOrderRequest.ListOrder == null || !createTicketOrderRequest.ListOrder.Any())
        {
            throw new BadRequestException("Danh sách vé không được để trống");
        }
        
        // Kiểm tra tổng số lượng vé không vượt quá 10 vé
        int totalTicketsInOrder = createTicketOrderRequest.ListOrder.Sum(item => item.Quantity);
        if (totalTicketsInOrder > 10)
        {
            throw new BadRequestException("Bạn chỉ được mua tối đa 10 vé cho 1 triển lãm");
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
        
        // Kiểm tra tổng số vé mà tài khoản này đã mua cho triển lãm này
        var koiShowId = firstTicketType.KoiShowId;
        var ticketAlreadyPurchased = 0;
        
        // Lấy tất cả các đơn hàng đã thanh toán của tài khoản cho triển lãm này
        var paidOrders = await _unitOfWork.GetRepository<TicketOrder>()
            .GetListAsync(
                predicate: o => o.AccountId == accountId && 
                               o.Status == OrderStatus.Paid.ToString().ToLower() &&
                               o.TicketOrderDetails.Any(d => d.TicketType.KoiShowId == koiShowId),
                include: query => query.Include(o => o.TicketOrderDetails)
                                     .ThenInclude(d => d.TicketType)
            );
            
        // Tính tổng số vé đã mua
        foreach (var order in paidOrders)
        {
            foreach (var detail in order.TicketOrderDetails)
            {
                // Chỉ đếm vé của triển lãm này
                if (detail.TicketType.KoiShowId == koiShowId)
                {
                    ticketAlreadyPurchased += detail.Quantity;
                }
            }
        }
        
        // Kiểm tra nếu tổng số vé (đã mua + đang mua) vượt quá 10
        var newTicketsQuantity = createTicketOrderRequest.ListOrder.Sum(item => item.Quantity);
        if (ticketAlreadyPurchased + newTicketsQuantity > 10)
        {
            throw new BadRequestException($"Bạn chỉ được mua tối đa 10 vé cho một triển lãm. Bạn đã mua {ticketAlreadyPurchased} vé, chỉ còn có thể mua thêm {10 - ticketAlreadyPurchased} vé.");
        }
        
        // Kiểm tra xem show có bị hủy không
        if (firstTicketType.KoiShow.Status == Domain.Enums.ShowStatus.Cancelled.ToString().ToLower())
        {
            throw new BadRequestException("Triển lãm đã bị hủy. Không thể mua vé.");
        }
        
        // Chỉ kiểm tra trạng thái kết thúc của triển lãm
        if (firstTicketType.KoiShow.Status?.ToLower() == Domain.Enums.ShowStatus.Finished.ToString().ToLower())
        {
            throw new BadRequestException("Triển lãm đã kết thúc. Không thể mua vé.");
        }
        
        // Sử dụng transaction với isolation level Serializable để đảm bảo tính nhất quán
        using (var transaction = await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.Serializable))
        {
            try
            {
                // BƯỚC 1: Tính toán totalAmount trước
                decimal totalAmount = 0;
                List<(TicketType ticketType, int quantity)> validatedTickets = new List<(TicketType, int)>();
                
                // Kiểm tra và tính toán tất cả trước khi thực hiện bất kỳ thay đổi nào
                foreach (var ticketTypeRequest in createTicketOrderRequest.ListOrder)
                {
                    var ticketTypeDb = await _unitOfWork.GetRepository<TicketType>()
                        .GetTrackedEntity(predicate: p => p.Id == ticketTypeRequest.TicketTypeId);
                        
                    if (ticketTypeDb == null)
                    {
                        throw new NotFoundException($"Không tìm thấy loại vé có ID {ticketTypeRequest.TicketTypeId}");
                    }
                    
                    if (ticketTypeDb.KoiShowId != firstTicketType.KoiShowId)
                    {
                        throw new BadRequestException("Không thể mua vé của nhiều triển lãm khác nhau trong cùng một đơn hàng");
                    }

                    if (ticketTypeRequest.Quantity > ticketTypeDb.AvailableQuantity)
                    {
                        throw new BadRequestException($"Loại vé '{ticketTypeDb.Name}' không đủ số lượng. Còn lại {ticketTypeDb.AvailableQuantity} vé.");
                    }
                    
                    // Tính totalAmount TRƯỚC
                    totalAmount += ticketTypeDb.Price * ticketTypeRequest.Quantity;
                    
                    // Lưu thông tin để xử lý sau
                    validatedTickets.Add((ticketTypeDb, ticketTypeRequest.Quantity));
                }

                // BƯỚC 2: Tạo đơn hàng với totalAmount đã tính
                var orderDate = VietNamTimeUtil.GetVietnamTime();
                var ticketOrder = new TicketOrder()
                {
                    Id = Guid.NewGuid(),
                    FullName = createTicketOrderRequest.FullName,
                    Email = createTicketOrderRequest.Email,
                    AccountId = accountId,
                    OrderDate = orderDate,
                    TransactionCode = transactionCode.ToString(),
                    TotalAmount = totalAmount, // Đã có giá trị ngay từ đầu
                    PaymentMethod = PaymentMethod.PayOs.ToString(),
                    Status = OrderStatus.Pending.ToString().ToLower(),
                };
                
                // Insert đơn hàng với TotalAmount đã tính sẵn
                await _unitOfWork.GetRepository<TicketOrder>().InsertAsync(ticketOrder);
                
                // BƯỚC 3: Cập nhật số lượng vé và tạo chi tiết đơn hàng
                var ticketOrderDetails = new List<TicketOrderDetail>();
                foreach (var (ticketTypeDb, quantity) in validatedTickets)
                {
                    // Cập nhật số lượng vé
                    ticketTypeDb.AvailableQuantity -= quantity;
                    _unitOfWork.GetRepository<TicketType>().UpdateAsync(ticketTypeDb);
                    
                    // Tạo chi tiết đơn hàng
                    var ticketOrderDetail = new TicketOrderDetail
                    {
                        TicketOrderId = ticketOrder.Id,
                        TicketTypeId = ticketTypeDb.Id,
                        Quantity = quantity,
                        UnitPrice = ticketTypeDb.Price
                    };
                    ticketOrderDetails.Add(ticketOrderDetail);
                }
                
                // Insert chi tiết đơn hàng
                await _unitOfWork.GetRepository<TicketOrderDetail>().InsertRangeAsync(ticketOrderDetails);
                
                // BƯỚC 4: Commit một lần duy nhất
                await _unitOfWork.CommitAsync();
                
                // Đặt lịch kiểm tra hết hạn thanh toán (3 phút + random để tránh xử lý đồng thời)
                var randomSeconds = new Random().Next(0, 30);
                _backgroundJobClient.Schedule(
                    () => HandleExpiredOrder(ticketOrder.Id),
                    TimeSpan.FromMinutes(6).Add(TimeSpan.FromSeconds(randomSeconds))
                );
                
                await transaction.CommitAsync();
                
                // Tạo link thanh toán
                var items = new List<ItemData>();
                foreach (var detail in ticketOrderDetails)
                {
                    var ticketTypeInfo = await _unitOfWork.GetRepository<TicketType>()
                        .SingleOrDefaultAsync(predicate: t => t.Id == detail.TicketTypeId);
                        
                    if (ticketTypeInfo != null)
                    {
                        var item = new ItemData(ticketTypeInfo.Name, detail.Quantity, (int)detail.UnitPrice);
                        items.Add(item);
                    }
                }
                
                var baseUrl = $"{AppConfig.AppSetting.BaseUrl}/api/v1/ticket-order" + "/call-back";
                var url = $"{baseUrl}?ticketOrderId={ticketOrder.Id}";
                
                // Tính thời gian hết hạn dựa trên thiết lập OrderDate + 2 phút
                var expiryTime = orderDate.AddMinutes(5);

                // Chuyển đổi sang Unix timestamp đảm bảo sử dụng múi giờ Việt Nam (UTC+7)
                var expiredAtTimestamp = new DateTimeOffset(
                    expiryTime,
                    new TimeSpan(7, 0, 0) // Chỉ định múi giờ Việt Nam (UTC+7)
                ).ToUnixTimeSeconds();
                
                _logger.LogInformation($"Order: {ticketOrder.Id}, OrderDate: {orderDate}, ExpiryTime: {expiryTime}, Timestamp: {expiredAtTimestamp}");
                
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
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi tạo đơn hàng");
                throw;
            }
        }
    }

    public async Task UpdateTicketOrder(Guid ticketOrderId, OrderStatus orderStatus)
    {
        using (var transaction = await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.Serializable))
        {
            try
            {
                // Sử dụng GetTrackedEntity để lấy đơn hàng có tracking
                var order = await _unitOfWork.GetRepository<TicketOrder>()
                    .GetTrackedEntity(
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
                    // Cập nhật trạng thái đơn hàng
                    order.Status = OrderStatus.Cancelled.ToString().ToLower();
                    _unitOfWork.GetRepository<TicketOrder>().UpdateAsync(order);
                    
                    // Hoàn trả số lượng vé
                    foreach (var detail in order.TicketOrderDetails)
                    {
                        var ticketTypeId = detail.TicketTypeId;
                        var quantity = detail.Quantity;
                        
                        // Lấy phiên bản mới nhất của TicketType từ database với tracking
                        var ticketType = await _unitOfWork.GetRepository<TicketType>()
                            .GetTrackedEntity(predicate: t => t.Id == ticketTypeId);
                            
                        if (ticketType != null)
                        {
                            // Cập nhật số lượng
                            ticketType.AvailableQuantity += quantity;
                            
                            // Cập nhật vào database (không commit)
                            _unitOfWork.GetRepository<TicketType>().UpdateAsync(ticketType);
                        }
                    }
                    
                    // Commit một lần duy nhất
                    await _unitOfWork.CommitAsync();
                    await transaction.CommitAsync();
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
                    
                    // Commit một lần duy nhất
                    await _unitOfWork.CommitAsync();
                    
                    // Gửi email có thể gọi ở ngoài transaction
                    _backgroundJobClient.Enqueue(() => _emailService.SendConfirmationTicket(ticketOrderId));
                }
                else if (newStatus == OrderStatus.Cancelled.ToString().ToLower())
                {
                    // Hoàn trả số lượng vé khi hủy đơn hàng
                    foreach (var detail in order.TicketOrderDetails)
                    {
                        var ticketTypeId = detail.TicketTypeId;
                        var quantity = detail.Quantity;
                        
                        // Lấy phiên bản mới nhất của TicketType từ database với tracking
                        var ticketType = await _unitOfWork.GetRepository<TicketType>()
                            .GetTrackedEntity(predicate: t => t.Id == ticketTypeId);
                            
                        if (ticketType != null)
                        {
                            // Cập nhật số lượng
                            ticketType.AvailableQuantity += quantity;
                            
                            // Cập nhật vào database (không commit)
                            _unitOfWork.GetRepository<TicketType>().UpdateAsync(ticketType);
                        }
                    }
                    
                    // Commit một lần duy nhất
                    await _unitOfWork.CommitAsync();
                }
                else
                {
                    // Commit các thay đổi khác
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
        using (var transaction = await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.Serializable))
        {
            try {
                // Lấy thông tin đơn hàng MỚI NHẤT trong transaction với tracking
                var order = await _unitOfWork.GetRepository<TicketOrder>()
                    .GetTrackedEntity(predicate: x => x.Id == orderId);

                if (order == null)
                {
                    _logger.LogWarning($"Không tìm thấy đơn hàng: {orderId}");
                    await transaction.CommitAsync();
                    return;
                }

                // Kiểm tra trạng thái đơn hàng
                if (order.Status != OrderStatus.Pending.ToString().ToLower())
                {
                    _logger.LogInformation($"Đơn hàng {orderId} có trạng thái {order.Status}, không cần xử lý hết hạn");
                    await transaction.CommitAsync();
                    return;
                }

                // Tính thời gian hết hạn: OrderDate + 2 phút
                var expiryTime = order.OrderDate.AddMinutes(5);
                var currentTime = VietNamTimeUtil.GetVietnamTime();
                
                // Chỉ xử lý các đơn hàng còn đang pending và đã quá hạn
                if (expiryTime <= currentTime)
                {
                    // Lưu thông tin cần thiết của đơn hàng trước khi xử lý
                    var accountId = order.AccountId;
                    var transactionCode = order.TransactionCode;
                    var totalAmount = order.TotalAmount;
                    
                    // Lấy tất cả chi tiết đơn hàng TRƯỚC KHI cập nhật trạng thái
                    var orderDetails = await _unitOfWork.GetRepository<TicketOrderDetail>()
                        .GetListAsync(predicate: od => od.TicketOrderId == orderId);
                    
                    // Cập nhật trạng thái đơn hàng thành "cancelled"
                    order.Status = OrderStatus.Cancelled.ToString().ToLower();
                    _unitOfWork.GetRepository<TicketOrder>().UpdateAsync(order);
                    
                    // Lấy và cập nhật số lượng cho từng loại vé
                    foreach (var detail in orderDetails)
                    {
                        var ticketTypeId = detail.TicketTypeId;
                        var quantity = detail.Quantity;
                        
                        // Lấy phiên bản mới nhất của TicketType từ database với tracking
                        var ticketType = await _unitOfWork.GetRepository<TicketType>()
                            .GetTrackedEntity(predicate: t => t.Id == ticketTypeId);
                            
                        if (ticketType != null)
                        {
                            // Cập nhật số lượng
                            ticketType.AvailableQuantity += quantity;
                            
                            // Cập nhật vào database (không commit)
                            _unitOfWork.GetRepository<TicketType>().UpdateAsync(ticketType);
                        }
                        else
                        {
                            _logger.LogWarning($"Không tìm thấy TicketType {ticketTypeId} khi xử lý hoàn vé");
                        }
                    }
                    
                    // Tìm thông tin show từ orderId (cần thiết cho thông báo)
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
                    
                    // COMMIT CHỈ MỘT LẦN ở cuối quá trình xử lý
                    await _unitOfWork.CommitAsync();
                    
                    // Gửi thông báo cho người dùng sau khi xử lý thành công
                    if (accountId != null)
                    {
                        try {
                            await _notificationService.SendNotification(
                                accountId,
                                "Đơn hàng vé đã hết hạn",
                                $"Đơn hàng vé tham quan triển lãm {showName} với mã giao dịch {transactionCode} trị giá {totalAmount:N0} VNĐ đã bị hủy do quá thời gian thanh toán. Bạn có thể tạo đơn hàng mới nếu muốn.",
                                NotificationType.Payment
                            );
                        }
                        catch (Exception notifyEx) {
                            // Log lỗi nhưng không làm ảnh hưởng đến việc xử lý đơn hàng
                            _logger.LogError(notifyEx, $"Lỗi khi gửi thông báo cho đơn hàng hết hạn: {orderId}");
                        }
                    }
                }
                else
                {
                    _logger.LogInformation($"Đơn hàng {orderId} chưa hết hạn: OrderDate={order.OrderDate}, ExpiryTime={expiryTime}, CurrentTime={currentTime}");
                }
                
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Lỗi khi xử lý đơn hàng hết hạn: {orderId}");
                throw; // Ném lại exception để Hangfire retry job
            }
        }
    }

    // Phương thức xử lý nghiệp vụ đơn hàng hết hạn
    private async Task HandleExpiredOrderImmediate(TicketOrder order, IDbContextTransaction transaction)
    {
        try
        {
            // Đọc lại trạng thái hiện tại của đơn hàng từ DB để đảm bảo không xử lý đồng thời
            var currentOrder = await _unitOfWork.GetRepository<TicketOrder>()
                .GetTrackedEntity(predicate: o => o.Id == order.Id);
            
            // Kiểm tra nếu đơn hàng không còn ở trạng thái pending
            if (currentOrder == null || currentOrder.Status != OrderStatus.Pending.ToString().ToLower())
            {
                // Đơn đã được xử lý ở nơi khác, không cần xử lý nữa
                _logger.LogInformation($"Đơn hàng {order.Id} đã được xử lý trước đó với trạng thái {currentOrder?.Status ?? "không xác định"}");
                return;
            }

            // Cập nhật trạng thái đơn hàng thành "cancelled"
            order.Status = OrderStatus.Cancelled.ToString().ToLower();
            _unitOfWork.GetRepository<TicketOrder>().UpdateAsync(order);
            
            // Lấy danh sách chi tiết đơn hàng
            var orderDetails = await _unitOfWork.GetRepository<TicketOrderDetail>()
                .GetListAsync(predicate: od => od.TicketOrderId == order.Id);

            // Hoàn trả số lượng vé 
            foreach (var detail in orderDetails)
            {
                // Lấy TicketTypeId từ detail
                var ticketTypeId = detail.TicketTypeId;
                var quantity = detail.Quantity;
                
                // Lấy phiên bản mới nhất của TicketType từ database với tracking
                var ticketType = await _unitOfWork.GetRepository<TicketType>()
                    .GetTrackedEntity(predicate: t => t.Id == ticketTypeId);
                    
                if (ticketType != null)
                {
                    // Cập nhật số lượng
                    ticketType.AvailableQuantity += quantity;
                    
                    // Cập nhật vào database (không commit)
                    _unitOfWork.GetRepository<TicketType>().UpdateAsync(ticketType);
                }
                else
                {
                    _logger.LogWarning($"Không tìm thấy loại vé {ticketTypeId} khi hoàn trả vé cho đơn hàng {order.Id}");
                }
            }

            // Commit CHỈ MỘT LẦN ở cuối
            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Lỗi khi xử lý hoàn trả vé cho đơn hàng {order.Id}");
            throw; // Ném lại exception để caller có thể xử lý rollback
        }
    }
}