﻿using KSMS.Application.Services;
using KSMS.Domain.Dtos;
using KSMS.Domain.Dtos.Requests.TicketOrder;
using KSMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KSMS.API.Controllers;
[Route("api/v1/ticket-order")]
[ApiController]
public class TicketOrderController : ControllerBase
{
    private readonly ITicketOrderService _ticketOrderService;

    public TicketOrderController(ITicketOrderService ticketOrderService)
    {
        _ticketOrderService = ticketOrderService;
    }

    [HttpPost("create-order")]
    [Authorize(Roles = "Member")]
    public async Task<ActionResult<ApiResponse<object>>> CreateOrder([FromBody] CreateTicketOrderRequest createTicketOrderRequest)
    {
        var createOrder = await _ticketOrderService.CreateTicketOrder(createTicketOrderRequest);
        return StatusCode(201, ApiResponse<object>.Created(createOrder, "Tạo thanh toán thành công"));
    }
    [HttpGet("call-back")]
    public async Task<IActionResult> Success([FromQuery] Guid ticketOrderId,[FromQuery] string status)
    {
        if (status == "CANCELLED")
        {
            await _ticketOrderService.UpdateTicketOrder(ticketOrderId, OrderStatus.Cancelled);
            return Redirect("ksms://app/fail?status=" + status);
        }
        await _ticketOrderService.UpdateTicketOrder(ticketOrderId, OrderStatus.Paid); 
        return Redirect("ksms://app/success?status=" + status);
    }

    [HttpGet("get-paging-orders")]
    [Authorize(Roles = "Member, Staff, Manager, Admin")]
    public async Task<ActionResult<ApiResponse<object>>> GetAllOrders([FromQuery]Guid? koiShowId, [FromQuery]OrderStatus? orderStatus,
        [FromQuery] int page = 1, [FromQuery]int size  = 10)
    {
        var orders = await _ticketOrderService.GetAllOrder(koiShowId, orderStatus, page, size);
        return Ok(ApiResponse<object>.Success(orders, "Lấy danh sách đơn hàng vé thành công"));
    }
    [HttpGet("get-order-details/{orderId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> GetOrderDetails(Guid orderId)
    {
        var orderDetails = await _ticketOrderService.GetOrderDetailByOrderId(orderId);
        return Ok(ApiResponse<object>.Success(orderDetails, "Lấy chi tiết đơn hàng vé thành công"));
    }
    [HttpGet("get-all-tickets/{orderDetailId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> GetTicketByOrderDetailId(Guid orderDetailId)
    {
        var orderDetails = await _ticketOrderService.GetTicketByOrderDetailId(orderDetailId);
        return Ok(ApiResponse<object>.Success(orderDetails, "Lấy danh sách vé thành công"));
    }
}