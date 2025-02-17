using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class Variety : BaseEntity
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }
    
    public virtual ICollection<CategoryVariety> CategoryVarieties { get; set; } = new List<CategoryVariety>();

    public virtual ICollection<KoiProfile> KoiProfiles { get; set; } = new List<KoiProfile>();
}
