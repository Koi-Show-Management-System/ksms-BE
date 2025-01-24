using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class Round : BaseEntity
{
    public Guid? CategoryId { get; set; }

    public string? Name { get; set; }

    public int? RoundOrder { get; set; }

    public string RoundType { get; set; } = null!;

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public decimal? MinScoreToAdvance { get; set; }

    public string? Status { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<GrandChampionContender> GrandChampionContenders { get; set; } = new List<GrandChampionContender>();

    public virtual ICollection<Score> Scores { get; set; } = new List<Score>();
}
