using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class Variety : BaseEntity
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual ICollection<KoiProfile> KoiProfiles { get; set; } = new List<KoiProfile>();

    public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}
