using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class KoiShow : BaseEntity
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
    
    public virtual ICollection<CompetitionCategory> CompetitionCategories { get; set; } = new List<CompetitionCategory>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Livestream> Livestreams { get; set; } = new List<Livestream>();

    public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();

    public virtual ICollection<ShowRule> ShowRules { get; set; } = new List<ShowRule>();

    public virtual ICollection<ShowStaff> ShowStaffs { get; set; } = new List<ShowStaff>();

    public virtual ICollection<ShowStatus> ShowStatuses { get; set; } = new List<ShowStatus>();

    public virtual ICollection<Sponsor> Sponsors { get; set; } = new List<Sponsor>();

    public virtual ICollection<Tank> Tanks { get; set; } = new List<Tank>();

    public virtual ICollection<TicketType> TicketTypes { get; set; } = new List<TicketType>();
}
