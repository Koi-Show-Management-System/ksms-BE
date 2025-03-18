using KSMS.Domain.Entities;

namespace KSMS.Application.Services;

public interface IEmailService
{
    Task SendRegistrationRejectionEmail(Guid registrationId);
    Task SendRegistrationConfirmationEmail(Guid registrationId);
    
    Task SendConfirmationTicket(Guid orderId);
    
    Task SendPaymentConfirmationEmail(Guid registrationPaymentId);
    Task SendRefundEmail(Guid registrationId);
}