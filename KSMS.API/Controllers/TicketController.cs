using Microsoft.AspNetCore.Mvc;
using KSMS.Application.Services;
using KSMS.Infrastructure.Utils;
using KSMS.Domain.Dtos.Requests;

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
        public async Task<IActionResult> VerifyTicketId([FromBody] Guid TicketId)
        {
            var isValid = await _ticketService.VerifyTicketIdAsync(TicketId);

            if (isValid)
            {
                return Ok(new { message = "Ticket hợp lệ!" });
            }

            return BadRequest(new { message = "Ticket không hợp lệ!" });
        }
    }
}
