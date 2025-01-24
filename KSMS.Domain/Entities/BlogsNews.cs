using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class BlogsNews : BaseEntity
{

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public Guid BlogCategoryId { get; set; }

    public Guid AccountId { get; set; }

    public string? ImgUrl { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual BlogCategory BlogCategory { get; set; } = null!;
}
