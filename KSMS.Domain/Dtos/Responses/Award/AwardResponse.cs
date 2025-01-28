using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.Award
{
    public class AwardResponse
    {
        public Guid Id;
        public Guid CategoryId { get; set; }

        public string Name { get; set; } = null!;

        public string? AwardType { get; set; }

        public decimal? PrizeValue { get; set; }

        public string? Description { get; set; }
    }
}
