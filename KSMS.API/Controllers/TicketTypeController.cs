using KSMS.Application.Services;
using KSMS.Domain.Dtos;
using KSMS.Domain.Dtos.Requests.Ticket;
using KSMS.Domain.Dtos.Requests.TicketType;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KSMS.API.Controllers;
[Route("api/v1/ticket-type")]
[ApiController]
public class TicketTypeController : ControllerBase
{
    private readonly ITicketTypeService _ticketTypeService;

    public TicketTypeController(ITicketTypeService ticketTypeService)
    {
        _ticketTypeService = ticketTypeService;
    }
    [Authorize(Roles = "Admin, Manager, Staff")]
    [HttpPost("create/{showId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> CreateTicketType(Guid showId,[FromBody] CreateTicketTypeRequest request)
    {
        await _ticketTypeService.CreateTicketTypeAsync(showId, request);
        return StatusCode(201, ApiResponse<object>.Created(null, "Tạo loại vé thành công"));
    }
    [Authorize(Roles = "Admin, Manager, Staff")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateTicketType(Guid id, [FromBody] UpdateTicketTypeRequestV2 request)
    {
        await _ticketTypeService.UpdateTicketTypeAsync(id, request);
        return Ok(ApiResponse<object>.Success(null, "Cập nhật loại vé thành công"));
    }
    [Authorize(Roles = "Admin, Manager, Staff")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteTicketType(Guid id)
    {
        await _ticketTypeService.DeleteTicketTypeAsync(id);
        return Ok(ApiResponse<object>.Success(null, "Xóa loại vé thành công"));
    }
}