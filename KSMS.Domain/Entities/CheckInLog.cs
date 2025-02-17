using System;
using System.Collections.Generic;

namespace KSMS.Domain.Entities;

public partial class CheckInLog
{
    public Guid Id { get; set; }

    public Guid? TicketId { get; set; }

    public Guid? RegistrationPaymentId { get; set; }

    public DateTime? CheckInTime { get; set; }

    public string? CheckInLocation { get; set; }

    public Guid? CheckedInBy { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }

    public virtual ShowStaff? CheckedInByNavigation { get; set; }

    public virtual RegistrationPayment? RegistrationPayment { get; set; }

    public virtual Ticket? Ticket { get; set; }
}
