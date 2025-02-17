using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class CriteriaCompetitionCategory : BaseEntity
{
    public Guid CompetitionCategoryId { get; set; }

    public Guid CriteriaId { get; set; }

    public string? RoundType { get; set; }

    public decimal? Weight { get; set; }

    public int? Order { get; set; }

    public virtual CompetitionCategory CompetitionCategory { get; set; } = null!;
}
