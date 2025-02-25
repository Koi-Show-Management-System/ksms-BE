using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.Variety
{
    public class CreateVarietyRequest
    {
        public string Name { get; set; } = null!;

        public string? Description { get; set; }
    }
}
