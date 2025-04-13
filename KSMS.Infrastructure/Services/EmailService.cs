using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Responses.RefereeAssignment;
using KSMS.Domain.Entities;
using KSMS.Domain.Enums;
using KSMS.Domain.Exceptions;
using KSMS.Infrastructure.Database;
using KSMS.Infrastructure.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShowStatus = KSMS.Domain.Enums.ShowStatus;

namespace KSMS.Infrastructure.Services;

public class EmailService : BaseService<EmailService>, IEmailService
{
    public EmailService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<EmailService> logger, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, httpContextAccessor)
    {
    }

    public async Task SendNewAccountNotificationEmail(Guid accountId, string password)
    {
        var account = await _unitOfWork.GetRepository<Account>()
            .SingleOrDefaultAsync(predicate: a => a.Id == accountId);
            
        if (account == null)
        {
            throw new NotFoundException("Không tìm thấy tài khoản");
        }
        
        bool mailSent;
        
        if (account.Role == RoleName.Referee.ToString())
        {
            mailSent = MailUtil.SendEmail(
                account.Email,
                MailUtil.ContentMailUtil.Title_NewRefereeAccount,
                MailUtil.ContentMailUtil.NewRefereeAccountNotification(account.FullName, account.Email, password),
                "");
        }
        else // Manager hoặc Staff
        {
            mailSent = MailUtil.SendEmail(
                account.Email,
                MailUtil.ContentMailUtil.Title_NewStaffAccount,
                MailUtil.ContentMailUtil.NewStaffAccountNotification(account.FullName, account.Email, password, account.Role),
                "");
        }
        
        if (!mailSent)
        {
            throw new BadRequestException("Không thể gửi email thông báo tạo tài khoản");
        }
    }

    public async Task SendRegistrationRejectionEmail(Guid registrationId, string rejectedReason)
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
            MailUtil.ContentMailUtil.RejectRegistration(registration, rejectedReason), "");
                
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
                    .ThenInclude(x => x.Tickets)
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

    public async Task SendPaymentConfirmationEmail(Guid registrationPaymentId)
    {
        var registrationPayment = await _unitOfWork.GetRepository<RegistrationPayment>()
            .SingleOrDefaultAsync(predicate: r => r.Id == registrationPaymentId,
                include: query => query
                    .Include(r => r.Registration).ThenInclude(r => r.Account)
                    .Include(r => r.Registration).ThenInclude(r => r.KoiShow)
                    .Include(r => r.Registration).ThenInclude(r => r.KoiMedia)
                    .Include(r => r.Registration).ThenInclude(r => r.CompetitionCategory)
                    .Include(r => r.Registration).ThenInclude(r => r.KoiProfile).ThenInclude(r => r.Variety));
        var sendMail = MailUtil.SendEmail(registrationPayment.Registration.Account.Email,
            MailUtil.ContentMailUtil.Title_ThankingForRegisterSh,
            MailUtil.ContentMailUtil.ConfirmingRegistration(registrationPayment.Registration), "");
        if (!sendMail)
        {
            throw new BadRequestException("Error sending confirmation email.");
        }
    }
    public async Task SendRefundEmail(Guid registrationId)
    {
        var registration = await _unitOfWork.GetRepository<Registration>()
            .SingleOrDefaultAsync(
                predicate: r => r.Id == registrationId,
                include: query => query
                    .Include(r => r.Account)
                    .Include(r => r.KoiProfile)
                    .ThenInclude(r => r.Variety)
                    .Include(r => r.CompetitionCategory)
                    .Include(r => r.RegistrationPayment)
                    .Include(r => r.KoiShow));

        if (registration == null)
        {
            throw new NotFoundException("Registration not found");
        }

        var sendMail = MailUtil.SendEmail(
            registration.Account.Email,
            "KOI SHOW - Thông báo hoàn phí đăng ký",
            MailUtil.ContentMailUtil.RefundRegistrationPayment(registration), 
            "");
            
        if (!sendMail)
        {
            throw new BadRequestException("Error sending refund notification email.");
        }
    }

    public async Task SendShowStatusChange(Guid showId)
    {
        var show = await _unitOfWork.GetRepository<KoiShow>()
            .SingleOrDefaultAsync(predicate: s => s.Id == showId);
        if (show == null)
        {
            throw new NotFoundException("Không tìm thấy triển lãm");
        }

        var staffList = await _unitOfWork.GetRepository<ShowStaff>()
            .GetListAsync(
                predicate: s => s.KoiShowId == showId,
                include: query => query.Include(s => s.Account));
        foreach (var staff in staffList)
        {
            string emailSubject;
            string emailBody;
            if (staff.Account.Role == RoleName.Staff.ToString())
            {
                emailSubject = MailUtil.ContentMailUtil.Title_ShowInternalReviewStaff;
                emailBody = MailUtil.ContentMailUtil.ShowInternalReviewNotificationForStaff(staff.Account.FullName, show);
            }
            else
            {
                emailSubject = MailUtil.ContentMailUtil.Title_ShowInternalReviewManager;
                emailBody = MailUtil.ContentMailUtil.ShowInternalReviewNotificationForManager(staff.Account.FullName, show); 
            }
            var mailSent = MailUtil.SendEmail(staff.Account.Email, emailSubject, emailBody, "");
            if (!mailSent)
            {
                throw new BadRequestException($"Không thể gửi email thông báo đến {staff.Account.Email}");
            }
        }
        
    }

    public async Task SendRefereeAssignmentNotification(Guid showId)
    {
        var show = await _unitOfWork.GetRepository<KoiShow>()
            .SingleOrDefaultAsync(predicate: s => s.Id == showId);
        if (show == null)
        {
            throw new NotFoundException("Không tìm thấy triển lãm");
        }
        var categories = await _unitOfWork.GetRepository<CompetitionCategory>()
            .GetListAsync(predicate: c => c.KoiShowId == showId);
        var categoryIds = categories.Select(c => c.Id).ToList();
        var refereeAssignments = await _unitOfWork.GetRepository<RefereeAssignment>()
            .GetListAsync(
                predicate: r => categoryIds.Contains(r.CompetitionCategoryId),
                include: query => query
                    .Include(r => r.RefereeAccount)
                    .Include(r => r.CompetitionCategory));
        var groupedAssignments = refereeAssignments
            .GroupBy(r => r.RefereeAccountId)
            .Select(group => new
            {
                RefereeId = group.Key,
                Referee = group.First().RefereeAccount,
                Assigments = group.Select(ra => new RefereeAssignmentInfo()
                {
                    CategoryName = ra.CompetitionCategory.Name,
                    RoundTypeName = GetRoundTypeName(ra.RoundType)
                }).ToList()
            });
        foreach (var referee in groupedAssignments)
        {
            if (referee.Referee.Email == null) continue;
            var mailSent = MailUtil.SendEmail(referee.Referee.Email,
                MailUtil.ContentMailUtil.Title_RefereeAssignment,
                MailUtil.ContentMailUtil.RefereeAssignmentNotification(referee.Referee.FullName, show, referee.Assigments), "");
            if (!mailSent)
            {
                throw new BadRequestException("Không thể gửi email thông báo đến trọng tài");
            }
        }
    }
    private string GetRoundTypeName(string roundType)
    {
        return roundType.ToLower() switch
        {
            "preliminary" => "Vòng sơ khảo",
            "evaluation" => "Vòng đánh giá chính",
            "final" => "Vòng chung kết",
            _ => "Khác"
        };
    }
}