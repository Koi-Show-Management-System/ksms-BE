using System;
using System.Collections.Generic;

namespace KSMS.Domain.Entities;

public partial class PaymentType
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<RegistrationPayment> RegistrationPayments { get; set; } = new List<RegistrationPayment>();

    public virtual ICollection<TicketOrder> TicketOrders { get; set; } = new List<TicketOrder>();
}
