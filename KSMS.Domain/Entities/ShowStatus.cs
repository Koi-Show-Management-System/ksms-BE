using System;
using System.Collections.Generic;

namespace KSMS.Domain.Entities;

public partial class ShowStatus
{
    public Guid Id { get; set; }

    public Guid KoiShowId { get; set; }

    public string StatusName { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; }

    public virtual KoiShow KoiShow { get; set; } = null!;
}
