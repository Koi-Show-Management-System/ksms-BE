using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class Feedback : BaseEntity
{
    public Guid AccountId { get; set; }

    public Guid KoiShowId { get; set; }

    public string Content { get; set; } = null!;

    public virtual Account Account { get; set; } = null!;

    public virtual KoiShow KoiShow { get; set; } = null!;
}
