using System;
using System.Collections.Generic;

namespace KSMS.Domain.Entities;

public partial class Notification
{
    public Guid Id { get; set; }

    public Guid? AccountId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime SentDate { get; set; }

    public virtual Account? Account { get; set; }
}
