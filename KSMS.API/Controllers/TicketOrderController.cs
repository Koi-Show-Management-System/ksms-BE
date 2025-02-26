using KSMS.Application.Services;
using KSMS.Domain.Dtos;
using KSMS.Domain.Dtos.Requests.TicketOrder;
using KSMS.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace KSMS.API.Controllers;
[Route("api/ticket-order")]
[ApiController]
public class TicketOrderController : ControllerBase
{
    private readonly ITicketOrderService _ticketOrderService;

    public TicketOrderController(ITicketOrderService ticketOrderService)
    {
        _ticketOrderService = ticketOrderService;
    }

    [HttpPost("create-order")]
    public async Task<ActionResult<ApiResponse<object>>> CreateOrder([FromBody] CreateTicketOrderRequest createTicketOrderRequest)
    {
        var createOrder = await _ticketOrderService.CreateTicketOrder(createTicketOrderRequest);
        return StatusCode(201, ApiResponse<object>.Created(createOrder, "Check out successfully"));
    }
    [HttpGet("call-back")]
    public async Task<IActionResult> Success([FromQuery] Guid ticketOrderId,[FromQuery] string status)
    {
        if (status == "CANCELLED")
        {
            await _ticketOrderService.UpdateTicketOrder(ticketOrderId, OrderStatus.Cancelled);
            return Redirect("http://localhost:5173/fail");
        }
        await _ticketOrderService.UpdateTicketOrder(ticketOrderId, OrderStatus.Paid); 
        return Redirect("http://localhost:5173/success");
    }
}