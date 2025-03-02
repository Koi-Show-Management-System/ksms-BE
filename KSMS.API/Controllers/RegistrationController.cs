using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.Registration;
using KSMS.Domain.Dtos.Responses.Registration;
using KSMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using KSMS.Domain.Dtos;
using KSMS.Domain.Models;
using KSMS.Domain.Entities;

namespace KSMS.API.Controllers
{
    [Route("api/v1/registration")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly    IRegistrationService _registrationService;

        public RegistrationController(IRegistrationService registrationService)
        {
            _registrationService = registrationService;
        }

        //[HttpPost("{showId:guid}/assign-all-fish")]
        //public async Task<ActionResult<ApiResponse<object>>> AssignAllFishToTank(Guid showId)
        //{
        //    try
        //    {
        //        await _registrationService.AssignAllFishToTankAndRound(showId);

        //        return Ok(ApiResponse<object>.Success(null, "Fish assigned successfully"));
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ApiResponse<object>.Fail(ex.Message));
        //    }
        //}
        

        [HttpPatch("assign-to-tank")]
        public async Task<ActionResult<ApiResponse<object>>> AssignMultipleFishesToTankAndRound(
    [FromBody] AssignFishesRequest request)
        {
            try
            {
                await _registrationService.AssignMultipleFishesToTankAndRound(request.RoundId, request.RegistrationIds);
                return Ok(ApiResponse<object>.Success(null, "Assigned fishes successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }



        [HttpPost("create")]
   //     [Authorize(Roles = "Member")]
        public async Task<ActionResult<ApiResponse<object>>> CreateRegistration([FromForm]CreateRegistrationRequest createRegistrationRequest)
        {
            var registration = await _registrationService.CreateRegistration(createRegistrationRequest);
            return StatusCode(201, ApiResponse<object>.Created(registration, "Create registration successfully"));
        }



        [HttpPost("checkout")]
        [Authorize(Roles = "Member")]
        public async Task<ActionResult<ApiResponse<CheckOutRegistrationResponse>>> CheckOut([FromQuery] Guid registrationId)
        {
            var result = await _registrationService.CheckOut(registrationId);
            return StatusCode(201, ApiResponse<CheckOutRegistrationResponse>.Created(result, "Create payment successfully"));
        }


        [HttpGet("call-back")]
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
        public async Task<ActionResult<ApiResponse<object>>> ToggleUserStatus(Guid id, RegistrationStatus status)
        {
            await _registrationService.UpdateStatusForRegistration(id, status);

            return Ok(ApiResponse<object>.Success(null, "Update registration status successfully"));
        }


        [HttpGet("get-paging-registration-for-current-account")]
        [Authorize(Roles = "Staff, Admin, Manager, Member")]
        public async Task<ActionResult<ApiResponse<object>>> GetAllRegistration([FromQuery]RegistrationFilter filter, [FromQuery]int page, [FromQuery]int size)
        {
            var registrations = await _registrationService.GetAllRegistrationForCurrentMember(filter, page, size);
            return Ok(ApiResponse<object>.Success(registrations, "Get List successfully"));
        }
    }
}
