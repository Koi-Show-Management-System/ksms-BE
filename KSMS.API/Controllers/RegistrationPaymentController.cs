using KSMS.Application.Services;
using KSMS.Domain.Dtos;
using KSMS.Domain.Dtos.Responses.RegistrationPayment;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace KSMS.API.Controllers
{
    [ApiController]
    [Route("api/registration-payment")]
    public class RegistrationPaymentController : ControllerBase
    {
        private readonly IRegistrationPaymentService _registrationPaymentService;

        public RegistrationPaymentController(IRegistrationPaymentService registrationPaymentService)
        {
            _registrationPaymentService = registrationPaymentService;
        }

        /// <summary>
        /// Get Registration Payment by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<RegistrationPaymentResponse>>> CheckinRegistration(Guid id)
        {
            var payment = await _registrationPaymentService.GetRegistrationPaymentByIdAsync(id);
            return Ok(ApiResponse<RegistrationPaymentResponse>.Success(payment, "Get registration payment successfully"));
        }
    }
}
