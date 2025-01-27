using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.Sponsor
{
    public class SponsorResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public string? LogoUrl { get; set; }

        public Guid ShowId { get; set; }
    }
}
