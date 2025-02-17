using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class Registration : BaseEntity
{
    public Guid KoiShowId { get; set; }

    public Guid KoiProfileId { get; set; }

    public string? RegistrationNumber { get; set; }

    public string RegisterName { get; set; } = null!;

    public decimal KoiSize { get; set; }

    public int KoiAge { get; set; }

    public Guid? CompetitionCategoryId { get; set; }

    public Guid AccountId { get; set; }

    public decimal RegistrationFee { get; set; }

    public string? Status { get; set; }

    public string? QrcodeData { get; set; }

    public string? Notes { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual CheckOutLog? CheckOutLog { get; set; }

    public virtual CompetitionCategory? CompetitionCategory { get; set; }

    public virtual ICollection<KoiMedium> KoiMedia { get; set; } = new List<KoiMedium>();

    public virtual KoiProfile KoiProfile { get; set; } = null!;

    public virtual KoiShow KoiShow { get; set; } = null!;

    public virtual RegistrationPayment? RegistrationPayment { get; set; }

    public virtual ICollection<RegistrationRound> RegistrationRounds { get; set; } = new List<RegistrationRound>();

    public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
