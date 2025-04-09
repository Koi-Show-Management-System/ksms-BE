using KSMS.Application.Services;
using KSMS.Domain.Dtos;
using KSMS.Domain.Dtos.Responses.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KSMS.API.Controllers;

[Route("api/v1/dashboard")]
[ApiController]
[Authorize(Roles = "Admin")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Lấy dữ liệu tổng quan cho dashboard
    /// </summary>
    /// <param name="koiShowId">ID của cuộc thi (không bắt buộc)</param>
    /// <returns>Dữ liệu tổng quan về doanh thu, chi phí, và số liệu thống kê</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<DashboardResponse>>> GetDashboardData([FromQuery] Guid? koiShowId = null)
    {
        var result = await _dashboardService.GetDashboardData(koiShowId);
        return Ok(ApiResponse<DashboardResponse>.Success(result, "Lấy dữ liệu dashboard thành công"));
    }
} 