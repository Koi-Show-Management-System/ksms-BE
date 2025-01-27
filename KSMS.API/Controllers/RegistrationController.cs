using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.Registration;
using KSMS.Domain.Dtos.Responses.Registration;
using KSMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KSMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly IRegistrationService _registrationService;

        public RegistrationController(IRegistrationService registrationService)
        {
            _registrationService = registrationService;
        }

        [HttpPost("payos")]
        [Authorize(Roles = "Member")]
        public async Task<ActionResult<RegistrationResponse>> CreateRegisWithPayOs([FromForm]CreateRegistrationRequest createRegistrationRequest)
        {
            return Created(nameof(CreateRegisWithPayOs), await _registrationService.CreateRegistrationWithPayOs(HttpContext.User, createRegistrationRequest));
        }
        [HttpGet("success")]
        public async Task<IActionResult> Success([FromQuery] Guid registrationPaymentId,[FromQuery] string status)
        {
            if (status == "CANCELLED")
            {
                await _registrationService.UpdateRegistrationPaymentStatusForPayOs(registrationPaymentId, RegistrationPaymentStatus.Cancelled);
                return Redirect("http://localhost:5173/fail");
            }
            await _registrationService.UpdateRegistrationPaymentStatusForPayOs(registrationPaymentId, RegistrationPaymentStatus.Paid); 
            return Redirect("http://localhost:5173/success");
        }
    }
}
