using System;
using System.Collections.Generic;

namespace KSMS.Domain.Entities;

public partial class Sponsor
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? LogoUrl { get; set; }

    public Guid ShowId { get; set; }

    public virtual Show Show { get; set; } = null!;
}
