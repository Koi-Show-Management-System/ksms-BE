using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class CategoryTank : BaseEntity
{

    public Guid CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public decimal? Capacity { get; set; }

    public string? Dimensions { get; set; }

    public string? Location { get; set; }

    public string? Status { get; set; }

    public decimal? Temperature { get; set; }

    public decimal? PhLevel { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<FishTankAssignment> FishTankAssignments { get; set; } = new List<FishTankAssignment>();
}
