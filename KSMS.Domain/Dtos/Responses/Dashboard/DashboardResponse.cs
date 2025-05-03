using System;
using System.Collections.Generic;

namespace KSMS.Domain.Dtos.Responses.Dashboard;

public class DashboardResponse
{
    public int TotalKoiShows { get; set; }
    public int TotalUsers { get; set; }
    public int TotalKoi { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalRefund { get; set; }
    public decimal NetProfit { get; set; }
    
    public List<KoiShowRevenueItem> KoiShowRevenues { get; set; } = new();
    public List<ProfitDistributionItem> ProfitDistribution { get; set; } = new();
}

public class KoiShowRevenueItem
{
    public Guid KoiShowId { get; set; }
    public string KoiShowName { get; set; }
    public decimal RegistrationRevenue { get; set; }
    public decimal RegistrationRefundAmount { get; set; }
    public decimal TicketRevenue { get; set; }
    public decimal TicketRefundAmount { get; set; }
    public decimal SponsorRevenue { get; set; }
    public decimal AwardRevenue { get; set; }
    public decimal NetProfit { get; set; }
}

public class ProfitDistributionItem
{
    public string KoiShowName { get; set; }
    public decimal Percentage { get; set; }
} 