using KSMS.Domain.Dtos.Responses.Ticket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Application.Services
{
    public interface ITicketService
    {

        Task<TicketResponse> GetTicketDetailByIdAsync(Guid ticketId);
        //  Task VerifyTicketIdAsync(ClaimsPrincipal claims, Guid qrCodeId);
    }
}
