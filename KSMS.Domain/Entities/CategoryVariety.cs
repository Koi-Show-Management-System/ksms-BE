using System;
using System.Collections.Generic;

namespace KSMS.Domain.Entities;

public partial class CategoryVariety
{
    public Guid Id { get; set; }

    public Guid VarietyId { get; set; }

    public Guid CompetitionCategoryId { get; set; }

    public virtual CompetitionCategory CompetitionCategory { get; set; } = null!;

    public virtual Variety Variety { get; set; } = null!;
}
