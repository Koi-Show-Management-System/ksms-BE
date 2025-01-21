using System;
using System.Collections.Generic;

namespace KSMS.Domain.Entities;

public partial class TicketOrderDetail
{
    public Guid Id { get; set; }

    public Guid TicketOrderId { get; set; }

    public Guid? TicketId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Amount { get; set; }

    public virtual ICollection<Qrcode> Qrcodes { get; set; } = new List<Qrcode>();

    public virtual Ticket? Ticket { get; set; }

    public virtual TicketOrder TicketOrder { get; set; } = null!;
}
