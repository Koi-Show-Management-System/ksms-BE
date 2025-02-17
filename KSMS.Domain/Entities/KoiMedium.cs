using System;
using System.Collections.Generic;

namespace KSMS.Domain.Entities;

public partial class KoiMedium
{
    public Guid Id { get; set; }

    public string MediaUrl { get; set; } = null!;

    public string MediaType { get; set; } = null!;

    public Guid? KoiProfileId { get; set; }

    public Guid? RegistrationId { get; set; }

    public virtual KoiProfile? KoiProfile { get; set; }

    public virtual Registration? Registration { get; set; }
}
