using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class Registration : BaseEntity
{
    public string? RegistrationNumber { get; set; }

    public Guid VarietyId { get; set; }

    public string Name { get; set; } = null!;

    public decimal Size { get; set; }

    public int Age { get; set; }

    public string Gender { get; set; } = null!;

    public string? Bloodline { get; set; }

    public string ImgUrl { get; set; } = null!;

    public string VideoUrl { get; set; } = null!;

    public Guid CategoryId { get; set; }

    public Guid AccountId { get; set; }

    public decimal RegistrationFee { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<FishTankAssignment> FishTankAssignments { get; set; } = new List<FishTankAssignment>();

    public virtual ICollection<GrandChampionContender> GrandChampionContenders { get; set; } = new List<GrandChampionContender>();

    public virtual RegistrationPayment? RegistrationPayment { get; set; }

    public virtual ICollection<Result> Results { get; set; } = new List<Result>();

    public virtual ICollection<Score> Scores { get; set; } = new List<Score>();

    public virtual Variety Variety { get; set; } = null!;

    public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
