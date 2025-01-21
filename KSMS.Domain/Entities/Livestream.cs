using System;
using System.Collections.Generic;

namespace KSMS.Domain.Entities;

public partial class Livestream
{
    public Guid Id { get; set; }

    public Guid ShowId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string StreamUrl { get; set; } = null!;

    public virtual Show Show { get; set; } = null!;
}
