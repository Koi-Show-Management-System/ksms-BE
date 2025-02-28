using KSMS.Domain.Dtos.Responses.Account;

using KSMS.Domain.Dtos.Responses.Criterion;
using KSMS.Domain.Dtos.Responses.Registration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.Score
{
    public class ScoreDetailResponse
    {
        public Guid RefereeAccountId { get; set; }

        public Guid RegistrationRoundId { get; set; }

        public decimal InitialScore { get; set; }

        public decimal TotalPointMinus { get; set; }

        public bool? IsPublic { get; set; }

        public string? Comments { get; set; }

        public virtual GetAllCriteriaResponse Criteria { get; set; } 

        public virtual AccountResponse RefereeAccount { get; set; } 


    }
}
