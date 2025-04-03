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
         private readonly ITicketService _ticketService;
        
         public TicketController(ITicketService ticketService)
         {
             _ticketService = ticketService;
         }
        
         [HttpPut("check-in/{ticketId:guid}")]
         [Authorize(Roles = "Staff, Admin, Manager")]
         public async Task<ActionResult<ApiResponse<string>>> CheckIn(Guid ticketId)
         {
             await _ticketService.CheckInTicket(ticketId);
             return Ok(ApiResponse<string>.Success(null, "Xác thực vé thành công"));
         }
         [HttpPut("mark-as-refunded/{ticketOrderId:guid}")]
         public async Task<ActionResult<ApiResponse<string>>> MarkAsRefunded(Guid ticketOrderId)
         {
             await _ticketService.RefundTicket(ticketOrderId);
             return Ok(ApiResponse<string>.Success(null, "Hoàn tiền cho đơn hàng này thành công"));
         }
         
         [HttpGet("get-info-by-qr-code")]
         [Authorize(Roles = "Staff, Admin, Manager")]
         public async Task<ActionResult<ApiResponse<object>>> GetInfo(Guid ticketId)
         {
             var ticketDetail = await _ticketService.GetTicketInfoByQrCode(ticketId);
             return Ok(ApiResponse<object>.Success(ticketDetail, "Lấy chi tiết vé thành công"));
         }

         
    }
}
