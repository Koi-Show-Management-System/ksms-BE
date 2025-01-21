using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class KoiProfile : BaseEntity
{
    public Guid OwnerId { get; set; }

    public Guid VarietyId { get; set; }

    public string? Name { get; set; }

    public decimal? Size { get; set; }

    public int? Age { get; set; }

    public string? Gender { get; set; }

    public string? Bloodline { get; set; }

    public string? Status { get; set; }

    public string? ImgUrl { get; set; }

    public string? VideoUrl { get; set; }

    public virtual Account Owner { get; set; } = null!;

    public virtual Variety Variety { get; set; } = null!;
}
