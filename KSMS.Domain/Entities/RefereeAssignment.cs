using System;
using System.Collections.Generic;

namespace KSMS.Domain.Entities;

public partial class RefereeAssignment
{
    public Guid Id { get; set; }

    public Guid CategoryId { get; set; }

    public Guid RefereeAccountId { get; set; }

    public DateTime AssignedAt { get; set; }

    public Guid AssignedBy { get; set; }

    public virtual Account AssignedByNavigation { get; set; } = null!;

    public virtual Category Category { get; set; } = null!;

    public virtual Account RefereeAccount { get; set; } = null!;
}
