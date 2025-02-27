using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.Sponsor
{
    public class GetAllSponsorResponse
    {

        public Guid KoiShowId { get; set; }
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public string? LogoUrl { get; set; }

        
    }
}
