using System;
using System.Collections.Generic;

namespace KSMS.Domain.Entities;

public partial class TicketType
{
    public Guid Id { get; set; }

    public Guid KoiShowId { get; set; }

    public string TicketType1 { get; set; } = null!;

    public decimal Price { get; set; }

    public int AvailableQuantity { get; set; }

    public virtual KoiShow KoiShow { get; set; } = null!;

    public virtual ICollection<TicketOrderDetail> TicketOrderDetails { get; set; } = new List<TicketOrderDetail>();
}
