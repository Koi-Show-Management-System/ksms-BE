using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class ShowRule : BaseEntity
{
    public Guid KoiShowId { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;
    
    public virtual KoiShow KoiShow { get; set; } = null!;
}
