using KSMS.Domain.Dtos.Requests.Criterion;
using KSMS.Domain.Dtos.Responses.Criterion;
using KSMS.Domain.Dtos.Responses.ErrorType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.CriteriaGroupRequest
{
        public class CriteriaCompetitionCategoryResponse
        {
                public Guid Id { get; set; }
                public Guid CompetitionCategoryId { get; set; }

                public Guid CriteriaId { get; set; }

                public string? RoundType { get; set; }

                public decimal? Weight { get; set; }

                public int? Order { get; set; }



                public virtual GetAllCriteriaGroupResponse Criteria { get; set; } = null!;



        }
}
