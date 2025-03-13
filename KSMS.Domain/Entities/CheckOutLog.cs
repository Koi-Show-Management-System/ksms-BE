using System;
using System.Collections.Generic;

namespace KSMS.Domain.Entities;

public partial class CheckOutLog
{
    public Guid Id { get; set; }

    public Guid? RegistrationId { get; set; }

    public string ImgCheckOut { get; set; } = null!;

    public DateTime? CheckOutTime { get; set; }

    public Guid? CheckedOutBy { get; set; }

    public string? Notes { get; set; }

    public virtual Account? CheckedOutByNavigation { get; set; }

    public virtual Registration? Registration { get; set; }
}
