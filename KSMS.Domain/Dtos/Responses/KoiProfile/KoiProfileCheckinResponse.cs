using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KSMS.Domain.Dtos.Responses.Variety;

namespace KSMS.Domain.Dtos.Responses.KoiProfile
{
    public class KoiProfileCheckinResponse
    {
        public string? Name { get; set; }  
        public string? Gender { get; set; }

        public string? Bloodline { get; set; }
        public VarietyResponse? Variety { get; set; }
    }
}
