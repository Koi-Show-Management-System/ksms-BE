using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class Score : BaseEntity
{
    public Guid RegistrationId { get; set; }

    public Guid RoundId { get; set; }

    public Guid RefereeAccountId { get; set; }

    public Guid CriteriaId { get; set; }

    public decimal? Score1 { get; set; }

    public string? Comments { get; set; }

    public string? Status { get; set; }

    public virtual Criterion Criteria { get; set; } = null!;

    public virtual Account RefereeAccount { get; set; } = null!;

    public virtual Registration Registration { get; set; } = null!;

    public virtual Round Round { get; set; } = null!;
}
