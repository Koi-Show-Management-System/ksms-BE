using System;
using System.Collections.Generic;

namespace KSMS.Domain.Entities;

public partial class ScoreDetailError
{
    public Guid Id { get; set; }

    public Guid ScoreDetailId { get; set; }

    public Guid ErrorTypeId { get; set; }

    public string Severity { get; set; } = null!;
    
    public decimal PointMinus { get; set; }

    public virtual ErrorType ErrorType { get; set; } = null!;

    public virtual ScoreDetail ScoreDetail { get; set; } = null!;
}
