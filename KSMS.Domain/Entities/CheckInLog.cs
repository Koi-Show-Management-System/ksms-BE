using System;
using System.Collections.Generic;

namespace KSMS.Domain.Entities;

public partial class CheckInLog
{
    public Guid Id { get; set; }

    public Guid QrcodeId { get; set; }

    public DateTime? CheckInTime { get; set; }

    public string? CheckInLocation { get; set; }

    public Guid? CheckedInBy { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }

    public virtual Account? CheckedInByNavigation { get; set; }

    public virtual Qrcode Qrcode { get; set; } = null!;
}
