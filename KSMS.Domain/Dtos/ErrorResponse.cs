﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos
{
    public record ErrorResponse
    {
        public int StatusCode { get; set; } = (int)HttpStatusCode.InternalServerError;

        public string? Error { get; set; }

        public DateTime TimeStamp { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
