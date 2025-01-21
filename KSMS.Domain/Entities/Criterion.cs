using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class Criterion : BaseEntity
{

    public Guid? CriteriaGroupId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal? MaxScore { get; set; }

    public decimal? Weight { get; set; }

    public int? Order { get; set; }
    

    public virtual CriteriaGroup? CriteriaGroup { get; set; }

    public virtual ICollection<Score> Scores { get; set; } = new List<Score>();
}
