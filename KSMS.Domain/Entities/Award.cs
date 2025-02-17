using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class Award : BaseEntity
{
    public Guid CompetitionCategoriesId { get; set; }

    public string Name { get; set; } = null!;

    public string? AwardType { get; set; }

    public decimal? PrizeValue { get; set; }

    public string? Description { get; set; }

    public virtual CompetitionCategory CompetitionCategories { get; set; } = null!;
}
