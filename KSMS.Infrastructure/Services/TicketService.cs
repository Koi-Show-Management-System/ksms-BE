using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Entities;
using KSMS.Infrastructure.Database;

namespace KSMS.Infrastructure.Services
{
    public class TicketService : ITicketService
    {
        private readonly IUnitOfWork<KoiShowManagementSystemContext> _unitOfWork;

        public TicketService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> VerifyTicketIdAsync(Guid ticketId)
        {
            var ticketRepository = _unitOfWork.GetRepository<Ticket>();  // Giả sử bạn có entity Ticket
            var ticket = await ticketRepository.SingleOrDefaultAsync(t => t.Id == ticketId ,null,null);

            return ticket != null;
        }
    }
}
