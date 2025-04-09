using KSMS.Domain.Dtos.Responses.Dashboard;

namespace KSMS.Application.Services;

public interface IDashboardService
{
    /// <summary>
    /// Lấy dữ liệu tổng quan cho dashboard
    /// </summary>
    /// <param name="koiShowId">ID cuộc thi (không bắt buộc)</param>
    /// <returns>Dữ liệu tổng quan cho dashboard</returns>
    Task<DashboardResponse> GetDashboardData(Guid? koiShowId = null);
} 