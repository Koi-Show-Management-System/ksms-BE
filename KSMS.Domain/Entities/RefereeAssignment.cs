using System;
using System.Collections.Generic;

namespace KSMS.Domain.Entities;

public partial class RefereeAssignment
{
    public Guid Id { get; set; }

    public Guid CompetitionCategoryId { get; set; }

    public Guid RefereeAccountId { get; set; }

    public string RoundType { get; set; } = null!;

    public DateTime AssignedAt { get; set; }

    public Guid AssignedBy { get; set; }

    public virtual Account AssignedByNavigation { get; set; } = null!;

    public virtual CompetitionCategory CompetitionCategory { get; set; } = null!;

    public virtual Account RefereeAccount { get; set; } = null!;
}
