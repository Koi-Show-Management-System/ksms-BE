using Microsoft.AspNetCore.Mvc;
using KSMS.Application.Services;
using KSMS.Infrastructure.Utils;
using KSMS.Domain.Dtos.Requests;
using Microsoft.AspNetCore.Authorization;
using KSMS.Domain.Dtos;

namespace KSMS.API.Controllers
{
    [Route("api/v1/ticket")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        // private readonly ITicketService _ticketService;
        //
        // public TicketController(ITicketService ticketService)
        // {
        //     _ticketService = ticketService;
        // }
        //
        //
        // [HttpPost("generate-qr")]
        // public ActionResult<ApiResponse<string>> GenerateQrCode([FromBody] Guid TicketId)
        // {
        //     var qrCodeBase64 = QrcodeUtil.GenerateQrCode(TicketId);
        //     return Ok(ApiResponse<string>.Success(qrCodeBase64, "Tạo mã QR thành công"));
        // }
        //
        //
        // [HttpGet("get-detail/{ticketId:guid}")]
        // [Authorize(Roles = "Staff")]
        // public async Task<ActionResult<ApiResponse<object>>> GetDetailTicketId(Guid ticketId)
        // {
        //     var ticketDetail = await _ticketService.GetTicketDetailByIdAsync(ticketId);
        //     return Ok(ApiResponse<object>.Success(ticketDetail, "Lấy chi tiết vé thành công"));
        // }

        //    [HttpPost("verify-ticket")]
        ////    [Authorize(Roles = "Staff")]
        //    public async Task<ActionResult<ApiResponse<string>>> VerifyTicketId([FromBody] Guid qrCodeId)
        //    {
        //        await _ticketService.VerifyTicketIdAsync(HttpContext.User, qrCodeId);
        //        return Ok(ApiResponse<string>.Success(null, "Xác thực vé thành công"));
        //    }
    }
}
