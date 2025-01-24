using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class CriteriaGroup : BaseEntity
{
    public Guid CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? RoundType { get; set; }

    public string? Description { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Criterion> Criteria { get; set; } = new List<Criterion>();
}
