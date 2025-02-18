using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class ErrorType : BaseEntity
{
    public Guid CriteriaId { get; set; }

    public string Name { get; set; } = null!;
    
    public virtual Criterion Criteria { get; set; } = null!;
    
    public virtual ICollection<ScoreDetailError> ScoreDetailErrors { get; set; } = new List<ScoreDetailError>();
}
