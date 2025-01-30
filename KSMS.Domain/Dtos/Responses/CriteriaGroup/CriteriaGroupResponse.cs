using KSMS.Domain.Dtos.Requests.Criterion;
using KSMS.Domain.Dtos.Responses.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.CriteriaGroupRequest
{
    public class CriteriaGroupResponse
    {
          
        public Guid CategoryId { get; set; }

        public string Name { get; set; } = null!;

        public string? RoundType { get; set; }

        public string? Description { get; set; }

        public virtual ICollection<CriterionResponse> Criteria { get; set; } = new List<CriterionResponse>();
    }
}
