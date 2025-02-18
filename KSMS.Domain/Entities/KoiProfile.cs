using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class KoiProfile : BaseEntity
{
    public Guid OwnerId { get; set; }

    public Guid VarietyId { get; set; }

    public string Name { get; set; } = null!;

    public decimal Size { get; set; }

    public int Age { get; set; }

    public string Gender { get; set; } = null!;

    public string Bloodline { get; set; } = null!;

    public string Status { get; set; } = null!;

    public bool? IsPublic { get; set; }

    public virtual ICollection<KoiMedium> KoiMedia { get; set; } = new List<KoiMedium>();

    public virtual Account Owner { get; set; } = null!;

    public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();

    public virtual Variety Variety { get; set; } = null!;
}
