using KSMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.Round
{
    public class CreateRoundRequest
    {
        public Guid? CategoryId { get; set; }

        public string? Name { get; set; }

        public int? RoundOrder { get; set; }

        public string RoundType { get; set; } = null!;

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public decimal? MinScoreToAdvance { get; set; }

        public string? Status { get; set; }

        
    }
}
