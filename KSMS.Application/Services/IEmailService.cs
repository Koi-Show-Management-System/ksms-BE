using KSMS.Domain.Entities;
using ShowStatus = KSMS.Domain.Enums.ShowStatus;

namespace KSMS.Application.Services;

public interface IEmailService
{
    Task SendRegistrationRejectionEmail(Guid registrationId);
    Task SendRegistrationConfirmationEmail(Guid registrationId);
    Task SendConfirmationTicket(Guid orderId);
    Task SendPaymentConfirmationEmail(Guid registrationPaymentId);
    Task SendRefundEmail(Guid registrationId);
    Task SendShowStatusChange(Guid showId);
    Task SendRefereeAssignmentNotification(Guid showId);
}