using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class ScoreDetail : BaseEntity
{
    public Guid RefereeAccountId { get; set; }

    public Guid RegistrationRoundId { get; set; }

    public decimal InitialScore { get; set; }

    public decimal TotalPointMinus { get; set; }

    public bool? IsPublic { get; set; }

    public string? Comments { get; set; }

    public virtual Account RefereeAccount { get; set; } = null!;

    public virtual RegistrationRound RegistrationRound { get; set; } = null!;

    public virtual ICollection<ScoreDetailError> ScoreDetailErrors { get; set; } = new List<ScoreDetailError>();
}
