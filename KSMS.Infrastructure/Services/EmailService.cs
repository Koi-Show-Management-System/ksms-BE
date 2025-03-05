using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Entities;
using KSMS.Domain.Exceptions;
using KSMS.Infrastructure.Database;
using KSMS.Infrastructure.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KSMS.Infrastructure.Services;

public class EmailService : BaseService<EmailService>, IEmailService
{
    public EmailService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<EmailService> logger, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, httpContextAccessor)
    {
    }
    public async Task SendRegistrationRejectionEmail(Guid registrationId)
    {
        var registration = await _unitOfWork.GetRepository<Registration>()
            .SingleOrDefaultAsync(
                predicate: r => r.Id == registrationId,
                include: query => query
                    .Include(r => r.Account)
                    .Include(r => r.KoiMedia)
                    .Include(r => r.KoiProfile)
                    .ThenInclude(r => r.Variety)
                    .Include(r => r.CompetitionCategory)
                    .Include(r => r.RegistrationPayment)
                    .Include(r => r.KoiShow));
        var sendMail = MailUtil.SendEmail(registration.Account.Email,
            "KOI SHOW - Thông báo từ chối đơn đăng ký",
            MailUtil.ContentMailUtil.RejectRegistration(registration), "");
                
        if (!sendMail)
        {
            throw new BadRequestException("Error sending rejection email.");
        }
    }

    public async Task SendRegistrationConfirmationEmail(Guid registrationId)
    {
        var registration = await _unitOfWork.GetRepository<Registration>()
            .SingleOrDefaultAsync(
                predicate: r => r.Id == registrationId,
                include: query => query
                    .Include(r => r.Account)
                    .Include(r => r.KoiMedia)
                    .Include(r => r.KoiProfile)
                    .ThenInclude(r => r.Variety)
                    .Include(r => r.CompetitionCategory)
                    .Include(r => r.RegistrationPayment)
                    .Include(r => r.KoiShow));
        var sendMail = MailUtil.SendEmail(registration.Account.Email,
            MailUtil.ContentMailUtil.Title_ApproveForRegisterSh,
            MailUtil.ContentMailUtil.ConfirmCategoryAssignment(registration), "");
                
        if (!sendMail)
        {
            throw new BadRequestException("Error sending confirmation email.");
        }
    }

    public async Task SendConfirmationTicket(Guid orderId)
    {
        var order = await _unitOfWork.GetRepository<TicketOrder>().SingleOrDefaultAsync(
            predicate: x => x.Id == orderId,
            include: query => query
                .Include(x => x.TicketOrderDetails)
                .ThenInclude(x => x.TicketType)
                .ThenInclude(x => x.KoiShow)
        );
        var sendMail = MailUtil.SendEmail(
            order.Email,
            "KOI SHOW - Xác nhận đơn hàng vé thành công",
            MailUtil.ContentMailUtil.ConfirmTicketOrder(order),
            ""
        );

        if (!sendMail)
        {
            throw new BadRequestException("Error sending confirmation email");
        }
    }
}