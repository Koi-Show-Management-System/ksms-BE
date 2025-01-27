using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.Variety
{
    public class VarietyResponse
    {
        public string Name { get; set; } = null!;

        public string? Description { get; set; }
    }
}
