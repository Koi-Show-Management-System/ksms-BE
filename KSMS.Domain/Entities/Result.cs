using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class Result : BaseEntity
{
    public Guid RegistrationId { get; set; }

    public decimal FinalScore { get; set; }

    public int? Rank { get; set; }

    public Guid AwardId { get; set; }

    public string? Comments { get; set; }

    public virtual Award Award { get; set; } = null!;

    public virtual Registration Registration { get; set; } = null!;
}
