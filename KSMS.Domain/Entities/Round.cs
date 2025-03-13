using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class Round : BaseEntity
{
    public Guid? CompetitionCategoriesId { get; set; }

    public string? Name { get; set; }

    public int? RoundOrder { get; set; }

    public string RoundType { get; set; } = null!;

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public int? NumberOfRegistrationToAdvance { get; set; }

    public string? Status { get; set; }

    public virtual CompetitionCategory? CompetitionCategories { get; set; }

    public virtual ICollection<RegistrationRound> RegistrationRounds { get; set; } = new List<RegistrationRound>();
}
