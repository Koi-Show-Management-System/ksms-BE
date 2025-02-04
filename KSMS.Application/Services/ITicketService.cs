using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Application.Services
{
    public interface ITicketService
    {
        Task<bool> VerifyTicketIdAsync(Guid ticketId);
    }
}
