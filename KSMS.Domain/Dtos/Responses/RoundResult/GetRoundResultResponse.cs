using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.RoundResult
{
    public class GetRoundResultResponse
    {
        public decimal TotalScore { get; set; }

        public bool? IsPublic { get; set; }

        public string? Comments { get; set; }

        public string? Status { get; set; }
    }
}
