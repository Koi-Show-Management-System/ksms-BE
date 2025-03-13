using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class Vote : BaseEntity
{
    public Guid AccountId { get; set; }

    public Guid RegistrationId { get; set; }

    public string? Prediction { get; set; }
    
    public virtual Account Account { get; set; } = null!;

    public virtual Registration Registration { get; set; } = null!;
}
