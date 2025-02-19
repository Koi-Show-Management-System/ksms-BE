using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.ScoreDetailError
{
    public class ScoreDetailErrorResponse
    {
        public Guid Id { get; set; }

        public Guid ScoreDetailId { get; set; }

        public Guid ErrorTypeId { get; set; }

        public string Severity { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public decimal PointMinus { get; set; }

 
    }
}
