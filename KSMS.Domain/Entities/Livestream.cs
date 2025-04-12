using System;
using System.Collections.Generic;

namespace KSMS.Domain.Entities;

public partial class Livestream
{
    public Guid Id { get; set; }

    public Guid KoiShowId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string CallId { get; set; } = null!;
    
    public string? Status { get; set; } // "Created", "Active", "Ended"

    public virtual KoiShow KoiShow { get; set; } = null!;
}
