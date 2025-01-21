using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class Category : BaseEntity
{

    public Guid ShowId { get; set; }

    public string Name { get; set; } = null!;

    public decimal? SizeMin { get; set; }

    public decimal? SizeMax { get; set; }

    public Guid? VarietyId { get; set; }

    public string? Description { get; set; }

    public int? MaxEntries { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string? Status { get; set; }
    

    public virtual ICollection<Award> Awards { get; set; } = new List<Award>();

    public virtual ICollection<CategoryTank> CategoryTanks { get; set; } = new List<CategoryTank>();

    public virtual ICollection<CriteriaGroup> CriteriaGroups { get; set; } = new List<CriteriaGroup>();

    public virtual ICollection<RefereeAssignment> RefereeAssignments { get; set; } = new List<RefereeAssignment>();

    public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();

    public virtual ICollection<Round> Rounds { get; set; } = new List<Round>();

    public virtual Show Show { get; set; } = null!;

    public virtual Variety? Variety { get; set; }
}
