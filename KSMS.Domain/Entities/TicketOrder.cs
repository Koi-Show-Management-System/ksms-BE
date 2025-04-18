using System;
using System.Collections.Generic;

namespace KSMS.Domain.Entities;

public partial class TicketOrder
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public Guid AccountId { get; set; }

    public DateTime OrderDate { get; set; }

    public string TransactionCode { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public string? PaymentMethod { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }

    public string? PaymentUrl { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<TicketOrderDetail> TicketOrderDetails { get; set; } = new List<TicketOrderDetail>();
}
