using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class Show : BaseEntity
{
    public string Name { get; set; } = null!;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime? StartExhibitionDate { get; set; }

    public DateTime? EndExhibitionDate { get; set; }

    public string? Location { get; set; }

    public string? Description { get; set; }

    public DateOnly? RegistrationDeadline { get; set; }

    public int? MinParticipants { get; set; }

    public int? MaxParticipants { get; set; }

    public bool? HasGrandChampion { get; set; }

    public bool? HasBestInShow { get; set; }

    public string? ImgUrl { get; set; }

    public decimal RegistrationFee { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Livestream> Livestreams { get; set; } = new List<Livestream>();

    public virtual ICollection<ShowRule> ShowRules { get; set; } = new List<ShowRule>();

    public virtual ICollection<ShowStaff> ShowStaffs { get; set; } = new List<ShowStaff>();

    public virtual ICollection<ShowStatistic> ShowStatistics { get; set; } = new List<ShowStatistic>();

    public virtual ICollection<ShowStatus> ShowStatuses { get; set; } = new List<ShowStatus>();

    public virtual ICollection<Sponsor> Sponsors { get; set; } = new List<Sponsor>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
