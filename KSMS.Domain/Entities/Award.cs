using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class Award : BaseEntity
{
    public Guid CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? AwardType { get; set; }

    public decimal? PrizeValue { get; set; }

    public string? Description { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Result> Results { get; set; } = new List<Result>();
}
