using System;
using System.Collections.Generic;

namespace KSMS.Domain.Entities;

public partial class Sponsor
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? LogoUrl { get; set; }

    public decimal InvestMoney { get; set; }

    public Guid KoiShowId { get; set; }

    public virtual KoiShow KoiShow { get; set; } = null!;
}
