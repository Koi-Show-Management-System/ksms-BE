using System;
using System.Collections.Generic;

namespace KSMS.Domain.Entities;

public partial class Ticket
{
    public Guid Id { get; set; }

    public Guid TicketOrderDetailId { get; set; }

    public string? QrcodeData { get; set; }

    public DateTime ExpiredDate { get; set; }

    public bool? IsUsed { get; set; }

    public virtual CheckInLog? CheckInLog { get; set; }

    public virtual TicketOrderDetail TicketOrderDetail { get; set; } = null!;
}
