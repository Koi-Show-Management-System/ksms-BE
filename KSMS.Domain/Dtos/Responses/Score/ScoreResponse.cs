using KSMS.Domain.Dtos.Responses.Account;
using KSMS.Domain.Dtos.Responses.Criterion;
using KSMS.Domain.Dtos.Responses.Registration;
using KSMS.Domain.Dtos.Responses.Round;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.Score
{
    public class ScoreResponse
    {
        public Guid RegistrationId { get; set; }

        public Guid RoundId { get; set; }

        public Guid RefereeAccountId { get; set; }

        public Guid CriteriaId { get; set; }

        public decimal? Score1 { get; set; }

        public string? Comments { get; set; }

        public string? Status { get; set; }

        public virtual CriterionResponse Criteria { get; set; } 

        public virtual AccountResponse RefereeAccount { get; set; } 

        public virtual RegistrationStaffResponse Registration { get; set; } 

        public virtual RoundResponse Round { get; set; } 
    }
}
