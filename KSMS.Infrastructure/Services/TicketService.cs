using System;
using System.Security.Claims;
using System.Threading.Tasks;
using KSMS.Application.Extensions;
using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Responses.Ticket;
using KSMS.Domain.Entities;
using KSMS.Domain.Exceptions;
using KSMS.Infrastructure.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace KSMS.Infrastructure.Services
{
    public class TicketService : BaseService<TicketService>, ITicketService
    {
        public TicketService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<TicketService> logger, IHttpContextAccessor httpContextAccessor)
            : base(unitOfWork, logger, httpContextAccessor)
        {
        }

        public Task<TicketResponse> GetTicketDetailByIdAsync(Guid ticketId)
        {
            throw new NotImplementedException();
        }

        //public async Task VerifyTicketIdAsync(ClaimsPrincipal claims, Guid ticketId)
        //{
        //    var accountId = claims.GetAccountId();

        //    // Lấy Ticket từ hệ thống
        //    var ticket = await _unitOfWork.GetRepository<Ticket>()
        //        .SingleOrDefaultAsync(predicate: x => x.Id == ticketId
        //        , include: query => query.Include(x => x.Ticket)
        //                 .ThenInclude(x => x.TicketOrderDetail)
        //               );

        //    if (ticket == null)
        //    {
        //        throw new NotFoundException("Ticket not found");
        //    }

        //    // Kiểm tra xem Ticket có hợp lệ không
        //    if (ticket.Status != "Active")
        //    {
        //        throw new BadRequestException("Ticket is not active");
        //    }

        //    // Tạo log check-in
        //    var checkInLog = new CheckInLog()
        //    {
        //        TicketId = ticket.Id,
        //        RegistrationPaymentId = ticket.RegistrationId,
        //        CheckInTime = DateTime.Now,
        //        CheckInLocation = ticket.Show.Location, // Assuming Show has a location field
        //        CheckedInBy = accountId,
        //        Notes = "Checked in successfully"
        //    };

        //    // Lưu log vào bảng CheckInLogs
        //    await _unitOfWork.GetRepository<CheckInLog>().InsertAsync(checkInLog);
        //    await _unitOfWork.CommitAsync();
        //}
    }
}
