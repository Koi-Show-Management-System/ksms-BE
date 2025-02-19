using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.Sponsor
{
    public class UpdateSponsorRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? LogoUrl { get; set; }
        public Guid koiShowId { get; set; }
    }
}
