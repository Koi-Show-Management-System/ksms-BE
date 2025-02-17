using System;
using System.Collections.Generic;

namespace KSMS.Domain.Entities;

public partial class ShowStaff
{
    public Guid Id { get; set; }

    public Guid KoiShowId { get; set; }

    public Guid AccountId { get; set; }

    public Guid AssignedBy { get; set; }

    public DateTime AssignedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Account AssignedByNavigation { get; set; } = null!;

    public virtual ICollection<CheckInLog> CheckInLogs { get; set; } = new List<CheckInLog>();

    public virtual ICollection<CheckOutLog> CheckOutLogs { get; set; } = new List<CheckOutLog>();

    public virtual KoiShow KoiShow { get; set; } = null!;

    public virtual ICollection<Tank> Tanks { get; set; } = new List<Tank>();
}
