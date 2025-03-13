using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class CompetitionCategory : BaseEntity
{
    public Guid KoiShowId { get; set; }

    public string Name { get; set; } = null!;

    public decimal? SizeMin { get; set; }

    public decimal? SizeMax { get; set; }

    public string? Description { get; set; }

    public int? MaxEntries { get; set; }

    public decimal RegistrationFee { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string? Status { get; set; }
    

    public virtual ICollection<Award> Awards { get; set; } = new List<Award>();

    public virtual ICollection<CategoryVariety> CategoryVarieties { get; set; } = new List<CategoryVariety>();

    public virtual ICollection<CriteriaCompetitionCategory> CriteriaCompetitionCategories { get; set; } = new List<CriteriaCompetitionCategory>();

    public virtual KoiShow KoiShow { get; set; } = null!;

    public virtual ICollection<RefereeAssignment> RefereeAssignments { get; set; } = new List<RefereeAssignment>();

    public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();

    public virtual ICollection<Round> Rounds { get; set; } = new List<Round>();

    public virtual ICollection<Tank> Tanks { get; set; } = new List<Tank>();
}
