﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Pagination
{
    public interface IPaginate<TResult>
    {
        int Size { get; }
        int Page { get; }
        int Total { get; }
        int TotalPages { get; }
        IList<TResult> Items { get; }
    }
}
