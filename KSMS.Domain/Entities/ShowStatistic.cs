using System;
using System.Collections.Generic;

namespace KSMS.Domain.Entities;

public partial class ShowStatistic
{
    public Guid Id { get; set; }

    public Guid? ShowId { get; set; }

    public string MetricName { get; set; } = null!;

    public decimal MetricValue { get; set; }

    public virtual Show? Show { get; set; }
}
