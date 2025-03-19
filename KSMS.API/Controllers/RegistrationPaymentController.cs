using KSMS.Application.Services;
using KSMS.Domain.Dtos;
using KSMS.Domain.Dtos.Responses.RegistrationPayment;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace KSMS.API.Controllers
{
    [ApiController]
    [Route("api/v1/registration-payment")]
    public class RegistrationPaymentController : ControllerBase
    {
        private readonly IRegistrationPaymentService _registrationPaymentService;

        public RegistrationPaymentController(IRegistrationPaymentService registrationPaymentService)
        {
            _registrationPaymentService = registrationPaymentService;
        }
        [HttpGet("checkin-info/{id:guid}")]
        public async Task<ActionResult<ApiResponse<RegistrationPaymentResponse>>> CheckinRegistration(Guid id)
        {
            var payment = await _registrationPaymentService.GetRegistrationPaymentByIdAsync(id);
            return Ok(ApiResponse<RegistrationPaymentResponse>.Success(payment, "Get registration for check in"));
        }
    }
}
