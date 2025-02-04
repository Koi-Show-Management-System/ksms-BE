using Microsoft.AspNetCore.Mvc;
using KSMS.Application.Services;
using KSMS.Infrastructure.Utils;
using KSMS.Domain.Dtos.Requests;
using Microsoft.AspNetCore.Authorization;

namespace KSMS.API.Controllers
{
    [Route("api/ticket")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

      
        [HttpPost("generate-qr")]
        public IActionResult GenerateQrCode([FromBody] Guid TicketId)
        {
            var qrCodeBase64 = QrcodeUtil.GenerateQrCode(TicketId);
            return Ok(new { qrCode = qrCodeBase64 });
        }

       
        [HttpPost("verify-ticket")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> VerifyTicketId([FromBody] Guid qrCodeId)
        {
            await _ticketService.VerifyTicketIdAsync(HttpContext.User, qrCodeId);

            return Ok(new { message = "Scan QR Successfully" });
        }
    }
}
