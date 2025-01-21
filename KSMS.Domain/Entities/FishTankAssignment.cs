using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class FishTankAssignment : BaseEntity
{

    public Guid RegistrationId { get; set; }

    public Guid? TankId { get; set; }

    public DateTime? CheckInTime { get; set; }

    public DateTime? CheckOutTime { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }
    

    public virtual Registration Registration { get; set; } = null!;

    public virtual CategoryTank? Tank { get; set; }
}
