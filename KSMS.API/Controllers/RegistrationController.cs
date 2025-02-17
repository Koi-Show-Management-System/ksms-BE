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

        [HttpPost("create")]
        [Authorize(Roles = "Member")]
        public async Task<ActionResult<object>> CreateRegistration([FromForm]CreateRegistrationRequest createRegistrationRequest)
        {
            return Created(nameof(CreateRegistration), await _registrationService.CreateRegistration(createRegistrationRequest));
        }
        [HttpPost("checkout")]
        [Authorize(Roles = "Member")]
        public async Task<ActionResult<CheckOutRegistrationResponse>> CheckOut([FromQuery] Guid registrationId)
        {
            return Created(nameof(CheckOut), await _registrationService.CheckOut(registrationId));
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
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> ToggleUserStatus(Guid id, RegistrationStatus status)
        {
            await _registrationService.UpdateStatusForRegistration(id, status);
            return NoContent();
        
        }
    }
}
