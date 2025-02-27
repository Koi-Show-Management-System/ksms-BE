using KSMS.Domain.Dtos.Requests.Ticket;
using KSMS.Domain.Dtos.Requests.TicketOrder;
using KSMS.Domain.Dtos.Responses.Ticket;
using KSMS.Domain.Dtos.Responses.TicketOrder;
using KSMS.Domain.Enums;
using KSMS.Domain.Pagination;

namespace KSMS.Application.Services;

public interface ITicketOrderService
{
    Task<CheckOutTicketResponse> CreateTicketOrder(CreateTicketOrderRequest createTicketOrderRequest);

    Task UpdateTicketOrder(Guid ticketOrderId, OrderStatus orderStatus);

    Task<Paginate<GetAllOrderResponse>> GetAllOrder(Guid? koiShowId, OrderStatus? orderStatus, int page, int size);

    Task<List<GetOrderDetailResponse>> GetOrderDetailByOrderId(Guid orderId);
    Task<List<GetTicketByOrderDetailResponse>> GetTicketByOrderDetailId(Guid orderDetailId);
}