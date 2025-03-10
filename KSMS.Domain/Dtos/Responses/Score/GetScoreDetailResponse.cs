using KSMS.Domain.Dtos.Responses.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.Score
{
    public class GetScoreDetailResponse
    {
        public virtual AccountGetResultRegistrationRoundResponse RefereeAccount { get; set; } = null!;

        public decimal TotalPointMinus { get; set; }
    }
}
