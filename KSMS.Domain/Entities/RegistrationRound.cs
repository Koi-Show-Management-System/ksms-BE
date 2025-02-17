using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class RegistrationRound : BaseEntity
{
    public Guid RegistrationId { get; set; }

    public Guid RoundId { get; set; }

    public DateTime? CheckInTime { get; set; }

    public DateTime? CheckOutTime { get; set; }

    public Guid TankId { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }

    public virtual Registration Registration { get; set; } = null!;

    public virtual Round Round { get; set; } = null!;

    public virtual ICollection<RoundResult> RoundResults { get; set; } = new List<RoundResult>();

    public virtual ICollection<ScoreDetail> ScoreDetails { get; set; } = new List<ScoreDetail>();

    public virtual Tank Tank { get; set; } = null!;
}
