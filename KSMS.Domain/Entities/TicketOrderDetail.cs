using System;
using System.Collections.Generic;

namespace KSMS.Domain.Entities;

public partial class TicketOrderDetail
{
    public Guid Id { get; set; }

    public Guid TicketOrderId { get; set; }

    public Guid TicketTypeId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Amount { get; set; }

    public virtual TicketOrder TicketOrder { get; set; } = null!;

    public virtual TicketType TicketType { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
