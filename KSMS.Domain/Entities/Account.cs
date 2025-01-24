using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class Account : BaseEntity
{
    public string? Email { get; set; }

    public string HashedPassword { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public string? Avatar { get; set; }

    public Guid? RoleId { get; set; }

    public string? Status { get; set; }

    public string? ConfirmationToken { get; set; }

    public bool? IsConfirmed { get; set; }
    
    public virtual ICollection<BlogsNews> BlogsNews { get; set; } = new List<BlogsNews>();

    public virtual ICollection<CheckInLog> CheckInLogs { get; set; } = new List<CheckInLog>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<KoiProfile> KoiProfiles { get; set; } = new List<KoiProfile>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<RefereeAssignment> RefereeAssignmentAssignedByNavigations { get; set; } = new List<RefereeAssignment>();

    public virtual ICollection<RefereeAssignment> RefereeAssignmentRefereeAccounts { get; set; } = new List<RefereeAssignment>();

    public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();

    public virtual Role? Role { get; set; }

    public virtual ICollection<Score> Scores { get; set; } = new List<Score>();

    public virtual ICollection<ShowStaff> ShowStaffAccounts { get; set; } = new List<ShowStaff>();

    public virtual ICollection<ShowStaff> ShowStaffAssignedByNavigations { get; set; } = new List<ShowStaff>();

    public virtual ICollection<TicketOrder> TicketOrders { get; set; } = new List<TicketOrder>();

    public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
