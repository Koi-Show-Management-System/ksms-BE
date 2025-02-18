using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.KoiProfile
{
    public class KoiProfileResponse
    {
        public string? Name { get; set; }

        public decimal? Size { get; set; }

        public int? Age { get; set; }

        public string? Gender { get; set; }

        public string? Bloodline { get; set; }

        public string? Status { get; set; }
        
    }
}
