using System;
using System.Collections.Generic;

namespace KSMS.Domain.Entities;

public partial class TicketOrder
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public Guid AccountId { get; set; }

    public Guid? PaymentTypeId { get; set; }

    public DateTime OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string? PaymentMethod { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual PaymentType? PaymentType { get; set; }

    public virtual ICollection<TicketOrderDetail> TicketOrderDetails { get; set; } = new List<TicketOrderDetail>();
}
