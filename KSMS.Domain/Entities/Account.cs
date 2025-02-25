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

    public string Role { get; set; } = null!;

    public string? Status { get; set; }

    public string? ConfirmationToken { get; set; }

    public bool? IsConfirmed { get; set; }

    public string? ResetPasswordOtp { get; set; }

    public DateTime? ResetPasswordOtpexpiry { get; set; }

    public virtual ICollection<BlogsNews> BlogsNews { get; set; } = new List<BlogsNews>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<KoiProfile> KoiProfiles { get; set; } = new List<KoiProfile>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<RefereeAssignment> RefereeAssignmentAssignedByNavigations { get; set; } = new List<RefereeAssignment>();

    public virtual ICollection<RefereeAssignment> RefereeAssignmentRefereeAccounts { get; set; } = new List<RefereeAssignment>();

    public virtual ICollection<Registration> RegistrationAccounts { get; set; } = new List<Registration>();

    public virtual ICollection<Registration> RegistrationCheckedInByNavigations { get; set; } = new List<Registration>();

    public virtual ICollection<ScoreDetail> ScoreDetails { get; set; } = new List<ScoreDetail>();

    public virtual ICollection<ShowStaff> ShowStaffAccounts { get; set; } = new List<ShowStaff>();

    public virtual ICollection<ShowStaff> ShowStaffAssignedByNavigations { get; set; } = new List<ShowStaff>();

    public virtual ICollection<TicketOrder> TicketOrders { get; set; } = new List<TicketOrder>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
