using KSMS.Domain.Dtos.Requests.Ticket;
using KSMS.Domain.Dtos.Requests.TicketType;

namespace KSMS.Application.Services;

public interface ITicketTypeService
{
    Task CreateTicketTypeAsync(Guid koiShowId, CreateTicketTypeRequest request);
    
    Task UpdateTicketTypeAsync(Guid id, UpdateTicketTypeRequestV2 request);
    
    Task DeleteTicketTypeAsync(Guid id);
}