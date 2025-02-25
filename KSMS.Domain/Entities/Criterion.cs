using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class Criterion : BaseEntity
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int? Order { get; set; }

    public virtual ICollection<CriteriaCompetitionCategory> CriteriaCompetitionCategories { get; set; } = new List<CriteriaCompetitionCategory>();

    public virtual ICollection<ErrorType> ErrorTypes { get; set; } = new List<ErrorType>();
}
