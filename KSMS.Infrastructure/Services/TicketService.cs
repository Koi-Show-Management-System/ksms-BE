using System.Security.Claims;
using KSMS.Application.Extensions;
using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Entities;
using KSMS.Domain.Exceptions;
using KSMS.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KSMS.Infrastructure.Services
{
    public class TicketService : BaseService<TicketService>,ITicketService
    {
        public TicketService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<TicketService> logger) : base(unitOfWork, logger)
        {
        }
        public async Task VerifyTicketIdAsync(ClaimsPrincipal claims, Guid qrCodeId)
        {
            var accountId = claims.GetAccountId();
            var qrCode = await _unitOfWork.GetRepository<Qrcode>()
                .SingleOrDefaultAsync(predicate: x => x.Id == qrCodeId,
                    include: query =>
                        query.Include(x => x.TicketOrderDetail)
                            .ThenInclude(x => x.Ticket)
                            .ThenInclude(x => x.Show));
                
            if (qrCode.IsActive == false)
            {
                throw new BadRequestException("QR Code is used");
            }
            if (qrCode.ExpiryDate < DateTime.Now)
            {
                throw new BadRequestException("QR Code has expired");
            }

            qrCode.IsActive = false;
            _unitOfWork.GetRepository<Qrcode>().UpdateAsync(qrCode);
            await _unitOfWork.CommitAsync();
            var checkinLogs = new CheckInLog()
            {
                QrcodeId = qrCode.Id,
                CheckInTime = DateTime.Now,
                CheckedInBy = accountId,
                CheckInLocation = qrCode.TicketOrderDetail.Ticket.Show.Location,
                Status = "confirm",

            };
            await _unitOfWork.GetRepository<CheckInLog>().InsertAsync(checkinLogs);
            await _unitOfWork.CommitAsync();
        }

        
    }
}
