using KSMS.Application.Services;
using KSMS.Domain.Dtos;
using KSMS.Domain.Dtos.Requests.Show;
using KSMS.Domain.Dtos.Responses.KoiShow;
using KSMS.Domain.Dtos.Responses.Variety;
using KSMS.Domain.Enums;
using KSMS.Domain.Exceptions;
using KSMS.Domain.Pagination;
using KSMS.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KSMS.API.Controllers
{
    [ApiController]
    [Route("api/v1/koi-show")]
    public class ShowController : ControllerBase
    {
        private readonly IShowService _showService;

        public ShowController(IShowService showService)
        {
            _showService = showService;
        }
        [HttpPost("create")]
        public async Task<ActionResult<ApiResponse<object>>> CreateShow([FromBody] CreateShowRequest createShowRequest)
        {
             await _showService.CreateShowAsync(createShowRequest);
            return StatusCode(201, ApiResponse<object>.Created(null, "Tạo triển lãm thành công"));
        }
        
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> GetShowById(Guid id)
        {
            var show = await _showService.GetShowDetailByIdAsync(id);

            return Ok(ApiResponse<object>.Success(show, "Lấy thông tin triển lãm thành công"));
        }
        

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateShow(Guid id, [FromBody] UpdateShowRequestV2 request)
        {
            await _showService.UpdateShowV2(id, request);
            return Ok(ApiResponse<object>.Success(null, "Cập nhật triển lãm thành công"));
        }

        [HttpGet("paged")]
        public async Task<ActionResult<ApiResponse<object>>> GetPagedShows([FromQuery] ShowStatus? showStatus, [FromQuery] int page = 1, [FromQuery] int size = 10 )
        {
            var shows = await _showService.GetPagedShowsAsync(page, size, showStatus);
            return Ok(ApiResponse<Paginate<PaginatedKoiShowResponse>>.Success(shows, "Lấy danh sách triển lãm thành công"));
        }
        
        [HttpGet("get-history-register-show")]
        [Authorize(Roles = "Member")]
        public async Task<ActionResult<ApiResponse<object>>> GetHistoryRegisterShow(
            [FromQuery]ShowStatus? showStatus,
            [FromQuery] int page = 1,
            [FromQuery] int size = 10)
        {
            var shows = await _showService.GetMemberRegisterShowAsync(showStatus, page, size);
            return Ok(ApiResponse<object>.Success(shows, "Lấy lịch sử triển lãm đã đăng ký thành công"));
        }
        
        [HttpPut("update-show-status{id:guid}")]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateShowStatus(
            Guid id,
            [FromQuery]ShowStatus status,
            [FromQuery]string? cancellationReason)
        {
            await _showService.CancelShowAsync(id, status, cancellationReason);
            return Ok(ApiResponse<object>.Success(null, "Cập nhật trạng thái triển lãm thành công"));
        }
        
        [HttpPut("update-status-manually")]
        //[Authorize(Roles = "Admin, Manager")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateShowStatusManually(
            [FromQuery] Guid koiShowId,
            [FromQuery] string statusName)
        {
            var result = await _showService.UpdateShowStatusManually(koiShowId, statusName, true);
            
            if (result)
            {
                return Ok(ApiResponse<object>.Success(null, $"Kích hoạt trạng thái '{statusName}' thành công"));
            }

            return Ok(ApiResponse<object>.Success(null, $"Trạng thái '{statusName}' đã ở trạng thái yêu cầu"));
        }
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteShow(
            Guid id)
        {
            await _showService.DeleteShowAsync(id);
            return Ok(ApiResponse<object>.Success(null, "Xóa triển lãm thành công"));
        }
    }
}
