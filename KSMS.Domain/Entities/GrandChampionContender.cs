using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class GrandChampionContender : BaseEntity
{

    public Guid RoundId { get; set; }

    public Guid RegistrationId { get; set; }

    public string? QualificationType { get; set; }
    

    public virtual Registration Registration { get; set; } = null!;

    public virtual Round Round { get; set; } = null!;
}
