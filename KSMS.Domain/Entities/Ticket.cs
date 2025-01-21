using System;
using System.Collections.Generic;

namespace KSMS.Domain.Entities;

public partial class Ticket
{
    public Guid Id { get; set; }

    public Guid ShowId { get; set; }

    public string TicketType { get; set; } = null!;

    public decimal Price { get; set; }

    public int AvailableQuantity { get; set; }

    public virtual Show Show { get; set; } = null!;

    public virtual ICollection<TicketOrderDetail> TicketOrderDetails { get; set; } = new List<TicketOrderDetail>();
}
