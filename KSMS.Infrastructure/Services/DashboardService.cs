using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Responses.Dashboard;
using KSMS.Domain.Entities;
using KSMS.Domain.Enums;
using KSMS.Infrastructure.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KSMS.Infrastructure.Services;

public class DashboardService : BaseService<DashboardService>, IDashboardService
{
    public DashboardService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, 
        ILogger<DashboardService> logger, 
        IHttpContextAccessor httpContextAccessor) 
        : base(unitOfWork, logger, httpContextAccessor)
    {
    }

    public async Task<DashboardResponse> GetDashboardData(Guid? koiShowId = null)
    {
        var response = new DashboardResponse();

        // 1. Tính tổng số cuộc thi
        if (koiShowId.HasValue)
        {
            response.TotalKoiShows = 1; // Nếu chỉ truy vấn một cuộc thi
        }
        else
        {
            response.TotalKoiShows = await _unitOfWork.GetRepository<KoiShow>().CountAsync();
        }

        // 2. Tính tổng số người dùng
        response.TotalUsers = await _unitOfWork.GetRepository<Account>().CountAsync();
        
        // 3. Tính tổng số Koi
        response.TotalKoi = await _unitOfWork.GetRepository<KoiProfile>().CountAsync();

        // 4. Tính doanh thu, hoàn trả và lợi nhuận
        var showRevenues = await CalculateRevenueData(koiShowId);
        response.KoiShowRevenues = showRevenues;

        // 5. Tính tổng doanh thu và hoàn trả
        // Doanh thu tổng (chưa trừ hoàn trả)
        response.TotalRevenue = showRevenues.Sum(r => r.RegistrationRevenue + r.TicketRevenue + r.SponsorRevenue);
        
        // Tổng số tiền hoàn trả
        response.TotalRefund = showRevenues.Sum(r => r.RegistrationRefundAmount + r.TicketRefundAmount);
        
        
        // Lợi nhuận ròng = doanh thu ròng (trong trường hợp này không có chi phí)
        response.NetProfit = showRevenues.Sum(r => (r.RegistrationRevenue - r.RegistrationRefundAmount) + 
                                                   (r.TicketRevenue - r.TicketRefundAmount) + 
                                                   r.SponsorRevenue);

        // 6. Tính phân bổ lợi nhuận
        var totalNetProfit = response.NetProfit;
        if (totalNetProfit > 0)
        {
            response.ProfitDistribution = showRevenues
                .Where(r => r.NetProfit > 0)
                .Select(s => new ProfitDistributionItem
                {
                    KoiShowName = s.KoiShowName,
                    Percentage = Math.Round((s.NetProfit / totalNetProfit) * 100, 2)
                })
                .ToList();
        }

        return response;
    }

    private async Task<List<KoiShowRevenueItem>> CalculateRevenueData(Guid? koiShowId)
    {
        var revenueItems = new List<KoiShowRevenueItem>();
        
        // Lấy danh sách cuộc thi
        var koiShows = await _unitOfWork.GetRepository<KoiShow>()
            .GetListAsync(
                predicate: koiShowId.HasValue ? k => k.Id == koiShowId.Value : null,
                include: query => query.Include(k => k.Registrations)
                    .Include(k => k.Sponsors)
                    .Include(k => k.TicketTypes)
            );

        foreach (var show in koiShows)
        {
            var revenueItem = new KoiShowRevenueItem
            {
                KoiShowId = show.Id,
                KoiShowName = show.Name
            };

            // 1. Tính doanh thu từ đăng ký
            var validStatuses = new[] {
                RegistrationStatus.Pending.ToString().ToLower(),     // Đã thanh toán, đang chờ duyệt
                RegistrationStatus.Confirmed.ToString().ToLower(),   // Đã được duyệt
                RegistrationStatus.CheckIn.ToString().ToLower(),     // Đã check-in
                RegistrationStatus.PrizeWinner.ToString().ToLower(), // Đã đoạt giải
                RegistrationStatus.Competition.ToString().ToLower(), // Đang thi đấu
                RegistrationStatus.Eliminated.ToString().ToLower(),  // Đã bị loại nhưng vẫn tính doanh thu
                RegistrationStatus.CheckedOut.ToString().ToLower(),
                RegistrationStatus.Refunded.ToString().ToLower(),
                RegistrationStatus.Rejected.ToString().ToLower()// Đã check-out
            };

            var confirmedRegistrations = show.Registrations
                .Where(r => validStatuses.Contains(r.Status))
                .ToList();
            
            revenueItem.RegistrationRevenue = confirmedRegistrations.Sum(r => r.RegistrationFee);

            // 2. Tính số tiền hoàn trả từ đăng ký
            var refundedRegistrations = show.Registrations
                .Where(r => r.Status == RegistrationStatus.Refunded.ToString().ToLower())
                .ToList();
            
            revenueItem.RegistrationRefundAmount = refundedRegistrations.Sum(r => r.RegistrationFee);

            // 3. Tính doanh thu từ tài trợ
            revenueItem.SponsorRevenue = show.Sponsors.Sum(s => s.InvestMoney);

            // 4. Tính doanh thu và hoàn trả từ vé
            // Lấy danh sách TicketTypes của cuộc thi
            var ticketTypeIds = show.TicketTypes.Select(tt => tt.Id).ToList();

            // Lấy danh sách TicketOrderDetail liên quan đến các loại vé của cuộc thi
            var ticketOrderDetails = await _unitOfWork.GetRepository<TicketOrderDetail>()
                .GetListAsync(
                    predicate: tod => ticketTypeIds.Contains(tod.TicketTypeId),
                    include: query => query
                        .Include(tod => tod.TicketOrder)
                );

            // Lấy danh sách đơn hàng vé đã thanh toán
            var paidTicketOrders = ticketOrderDetails
                .Select(tod => tod.TicketOrder)
                .Where(to => to.Status?.ToLower() == "paid")
                .GroupBy(to => to.Id) // Nhóm theo ID để loại bỏ trùng lặp
                .Select(g => g.First()) // Lấy phần tử đầu tiên từ mỗi nhóm
                .ToList();


            // Tính doanh thu từ vé đã thanh toán (tổng giá trị các đơn hàng)
            revenueItem.TicketRevenue = paidTicketOrders.Sum(to => to.TotalAmount);

            // Lấy danh sách vé đã hoàn trả và đơn hàng của chúng
            var ticketOrderDetailIds = ticketOrderDetails.Select(tod => tod.Id).ToList();
            var refundedTickets = await _unitOfWork.GetRepository<Ticket>()
                .GetListAsync(
                    predicate: t => ticketOrderDetailIds.Contains(t.TicketOrderDetailId) && 
                                   t.Status.ToLower() == "refunded",
                    include: query => query.Include(t => t.TicketOrderDetail)
                                          .ThenInclude(tod => tod.TicketOrder)
                );
                
            // Lấy danh sách đơn hàng có vé đã hoàn tiền
            var refundedOrders = refundedTickets
                .Select(t => t.TicketOrderDetail.TicketOrder)
                .Distinct()
                .ToList();
                
            // Tính tổng tiền hoàn trả từ vé (dùng TotalAmount từ TicketOrder)
            revenueItem.TicketRefundAmount = refundedOrders.Sum(to => to.TotalAmount);

            // Tính lợi nhuận ròng (doanh thu thực tế đã trừ hoàn trả)
                                   
            // Hiện tại, lợi nhuận ròng bằng doanh thu ròng vì chưa tính các chi phí khác
            revenueItem.NetProfit = (revenueItem.RegistrationRevenue - revenueItem.RegistrationRefundAmount) + 
                                    (revenueItem.TicketRevenue - revenueItem.TicketRefundAmount) + 
                                    revenueItem.SponsorRevenue;

            revenueItems.Add(revenueItem);
        }

        return revenueItems;
    }
} 