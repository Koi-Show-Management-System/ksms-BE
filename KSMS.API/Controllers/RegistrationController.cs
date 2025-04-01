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
using KSMS.Domain.Dtos.Responses.Round;
using KSMS.Infrastructure.Utils;
using ShowStatus = KSMS.Domain.Enums.ShowStatus;

namespace KSMS.API.Controllers
{
    [Route("api/v1/registration")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly IRegistrationService _registrationService;

        public RegistrationController(IRegistrationService registrationService)
        {
            _registrationService = registrationService;
        }
        // generate mã qr cho từng đơn đăng kí để trọng tài quét
        [HttpGet("generate-qr-list-regisation-referree")]
        public async Task<ActionResult<ApiResponse<List<GetRegisByQrCodeResponse>>>> GenerateQrCodesForRegistrationsByKoiShow([FromQuery] Guid koiShowId)
        {
            var registrationIds = await _registrationService.GetRegistrationIdsByKoiShowAsync(koiShowId);
            var qrCodes = registrationIds.Select(id => new GetRegisByQrCodeResponse
            {
                RegistrationId = id,
                Qrcode = QrcodeUtil.GenerateQrCode(id)
            }).ToList();

            return Ok(ApiResponse<List<GetRegisByQrCodeResponse>>.Success(qrCodes, "QR Codes generated successfully"));
        }
        
       




        [HttpPost("create")]
        [Authorize(Roles = "Member")]
        public async Task<ActionResult<ApiResponse<object>>> CreateRegistration([FromForm]CreateRegistrationRequest createRegistrationRequest)
        {
            var registration = await _registrationService.CreateRegistration(createRegistrationRequest);
            return StatusCode(201, ApiResponse<object>.Created(registration, "Tạo đăng ký thành công"));
        }

        [HttpGet("find-suitable-category")]
        public async Task<ActionResult<ApiResponse<object>>> FindSuitableCategory(
            [FromQuery] Guid koiShowId,
            [FromQuery] Guid varietyId,
            [FromQuery] decimal size)
        {
            var result =
                await _registrationService.FindSuitableCategoryAsync(koiShowId, varietyId, size);
            return Ok(ApiResponse<object>.Success(result, "Đã tìm thấy hạng mục phù hợp cho hồ sơ đăng ký của bạn"));
            
        }

        [HttpPost("checkout/{registrationId:guid}")]
        [Authorize(Roles = "Member")]
        public async Task<ActionResult<ApiResponse<CheckOutRegistrationResponse>>> CheckOut(Guid registrationId)
        {
            var result = await _registrationService.CheckOut(registrationId);
            return StatusCode(201, ApiResponse<CheckOutRegistrationResponse>.Created(result, "Tạo thanh toán thành công"));
        }


        [HttpGet("call-back")]
        public async Task<IActionResult> Success([FromQuery] Guid registrationPaymentId,[FromQuery] string status)
        {
            if (status == "CANCELLED")
            {
                await _registrationService.UpdateRegistrationPaymentStatusForPayOs(registrationPaymentId, RegistrationPaymentStatus.Cancelled);
                return Redirect("ksms://app/fail?status=" + status);
            }
            await _registrationService.UpdateRegistrationPaymentStatusForPayOs(registrationPaymentId, RegistrationPaymentStatus.Paid); 
            return Redirect("ksms://app/success?status=" + status);
        }


        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Staff, Admin, Manager")]
        public async Task<ActionResult<ApiResponse<object>>> ToggleUserStatus(Guid id, [FromQuery] RegistrationStatus status,
            [FromQuery]string? rejectedReason,[FromQuery] RefundType? refundType)
        {
            await _registrationService.UpdateStatusForRegistration(id, status, rejectedReason, refundType);

            return Ok(ApiResponse<object>.Success(null, "Cập nhật trạng thái đăng ký thành công"));
        }
        [HttpGet("get-paging-registration-for-current-account")]
        [Authorize(Roles = "Staff, Admin, Manager, Member")]
        public async Task<ActionResult<ApiResponse<object>>> GetAllRegistration([FromQuery]RegistrationFilter filter, [FromQuery]int page = 1, [FromQuery]int size = 10)
        {
            var registrations = await _registrationService.GetAllRegistrationForCurrentMember(filter, page, size);
            return Ok(ApiResponse<object>.Success(registrations, "Lấy danh sách đơn đăng ký thành công"));
        }
        
        [HttpGet("get-page-history-registration")]
        [Authorize(Roles = "Member")]
        public async Task<ActionResult<ApiResponse<object>>> GetPagedRegistrationHistory(
            [FromQuery] RegistrationStatus? registrationStatus,
            [FromQuery] ShowStatus? showStatus,
            [FromQuery]int page = 1,
            [FromQuery]int size = 10)
        {
            var result =
                await _registrationService.GetPageRegistrationHistory(registrationStatus, showStatus, page, size);
            return Ok(ApiResponse<object>.Success(result, "Lấy lịch sử đơn đăng ký thành công"));
            
        }
        [HttpGet("get-show-member-detail/{showId:guid}")]
        [Authorize(Roles = "Member")]
        public async Task<ActionResult<ApiResponse<GetShowMemberDetailResponse>>>
            GetShowMemberDetail(Guid showId)
        {
            var result = await _registrationService.GetMemberRegisterShowDetail(showId);
            return Ok(ApiResponse<GetShowMemberDetailResponse>.Success(result, "Lấy danh sách đơn đăng ký thành công"));
        }
    }
}
