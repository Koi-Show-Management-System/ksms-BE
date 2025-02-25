using System;
using System.Collections.Generic;

namespace KSMS.Domain.Entities;

public partial class Ticket
{
    public Guid Id { get; set; }

    public Guid TicketOrderDetailId { get; set; }

    public string? QrcodeData { get; set; }

    public DateTime ExpiredDate { get; set; }

    public bool? IsCheckedIn { get; set; }

    public DateTime? CheckInTime { get; set; }

    public string? CheckInLocation { get; set; }

    public Guid? CheckedInBy { get; set; }

    public string? Status { get; set; }

    public virtual Account? CheckedInByNavigation { get; set; }

    public virtual TicketOrderDetail TicketOrderDetail { get; set; } = null!;
}
