using KSMS.Domain.Dtos.Requests.Criterion;
using KSMS.Domain.Dtos.Requests.Variety;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.CriteriaGroup
{
    public class UpdateCriteriaGroupRequest
    {
        public Guid Id { get; set; }
        public Guid CompetitionCategoryId { get; set; }

        public Guid CriteriaId { get; set; }
        public string Name { get; set; } = null!;
        public string? RoundType { get; set; }
        public decimal? Weight { get; set; }

        public int? Order { get; set; }

        public virtual UpdateCriterionRequest? Criterias { get; set; } 
 
    }
}

