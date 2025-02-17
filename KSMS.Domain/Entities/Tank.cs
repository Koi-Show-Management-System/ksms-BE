using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class Tank : BaseEntity
{
    public Guid KoiShowId { get; set; }

    public string Name { get; set; } = null!;

    public int Capacity { get; set; }

    public string? WaterType { get; set; }

    public decimal? Temperature { get; set; }

    public decimal? Phlevel { get; set; }

    public decimal? Size { get; set; }

    public string? Location { get; set; }

    public string? Status { get; set; }

    public Guid CreatedBy { get; set; }

    public virtual ShowStaff CreatedByNavigation { get; set; } = null!;

    public virtual KoiShow KoiShow { get; set; } = null!;

    public virtual ICollection<RegistrationRound> RegistrationRounds { get; set; } = new List<RegistrationRound>();
}
