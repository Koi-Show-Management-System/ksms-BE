using System;
using System.Collections.Generic;

namespace KSMS.Domain.Entities;

public partial class BlogCategory
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<BlogsNews> BlogsNews { get; set; } = new List<BlogsNews>();
}
