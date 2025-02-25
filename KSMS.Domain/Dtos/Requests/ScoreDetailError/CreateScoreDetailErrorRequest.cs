using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.ScoreDetailError
{
    public class CreateScoreDetailErrorRequest
    {
   //     public Guid Id { get; set; }

        public Guid ErrorTypeId { get; set; }

        public string Severity { get; set; } = null!;

        public decimal PointMinus { get; set; }
    }
}
