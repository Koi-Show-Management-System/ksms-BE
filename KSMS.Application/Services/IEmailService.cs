using KSMS.Domain.Entities;
using ShowStatus = KSMS.Domain.Enums.ShowStatus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KSMS.Application.Services;

public interface IEmailService
{
    Task SendRegistrationRejectionEmail(Guid registrationId, string rejectedReason);
    Task SendRegistrationConfirmationEmail(Guid registrationId);
    Task SendConfirmationTicket(Guid orderId);
    Task SendPaymentConfirmationEmail(Guid registrationPaymentId);
    Task SendRefundEmail(Guid registrationId);
    Task SendShowStatusChange(Guid showId);
    Task SendRefereeAssignmentNotification(Guid showId);
    Task SendNewAccountNotificationEmail(Guid accountId, string password);
}