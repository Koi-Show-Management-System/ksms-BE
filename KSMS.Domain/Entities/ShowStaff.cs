using System;
using System.Collections.Generic;

namespace KSMS.Domain.Entities;

public partial class ShowStaff
{
    public Guid Id { get; set; }

    public Guid ShowId { get; set; }

    public Guid AccountId { get; set; }

    public Guid AssignedBy { get; set; }

    public DateTime AssignedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Account AssignedByNavigation { get; set; } = null!;

    public virtual Show Show { get; set; } = null!;
}
