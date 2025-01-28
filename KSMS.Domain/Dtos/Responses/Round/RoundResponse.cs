using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.Round
{
    public class RoundResponse
    {
        public Guid Id { get; set; }
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
