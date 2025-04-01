using KSMS.Domain.Dtos.Responses.Ticket;

namespace KSMS.Application.Services;

public interface ITicketService
{
    Task<GetTicketInfoByQrCode> GetTicketInfoByQrCode(Guid id);
    Task RefundTicket(Guid ticketOrderId);
    Task CheckInTicket(Guid id);
}