using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class RoundResult : BaseEntity
{
    public Guid RegistrationRoundsId { get; set; }

    public decimal TotalScore { get; set; }

    public bool? IsPublic { get; set; }

    public string? Comments { get; set; }

    public string? Status { get; set; }

    public virtual RegistrationRound RegistrationRounds { get; set; } = null!;
}
