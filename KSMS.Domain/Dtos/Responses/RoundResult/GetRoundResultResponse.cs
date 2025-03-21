using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.RoundResult
{
    public class GetRoundResultResponse
    {
        public Guid Id { get; set; }
        public decimal TotalScore { get; set; }
        public bool? IsPublic { get; set; }

        public string? Comments { get; set; }

        public string? Status { get; set; }
        
        public DateTime? CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
    }
}
