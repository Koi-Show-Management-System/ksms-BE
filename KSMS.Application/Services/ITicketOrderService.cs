using KSMS.Domain.Dtos.Requests.Ticket;
using KSMS.Domain.Dtos.Requests.TicketOrder;
using KSMS.Domain.Dtos.Responses.TicketOrder;
using KSMS.Domain.Enums;

namespace KSMS.Application.Services;

public interface ITicketOrderService
{
    Task<CheckOutTicketResponse> CreateTicketOrder(CreateTicketOrderRequest createTicketOrderRequest);

    Task UpdateTicketOrder(Guid ticketOrderId, OrderStatus orderStatus);
}