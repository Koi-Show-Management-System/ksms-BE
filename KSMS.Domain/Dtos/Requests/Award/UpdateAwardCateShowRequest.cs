using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.Award
{
    public class UpdateAwardCateShowRequest
    {

        public Guid Id {  get; set; }
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public string? AwardType { get; set; }
        public decimal? PrizeValue { get; set; }
        public string? Description { get; set; }
    }
}
