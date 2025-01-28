using KSMS.Domain.Dtos.Requests.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.CriteriaGroup
{
    public class CriteriaGroupRequest
    {
        public Guid CategoryId { get; set; }

        public string Name { get; set; } = null!;

        public string? RoundType { get; set; }

        public string? Description { get; set; }

        public virtual ICollection<CriterionRequest> Criterias { get; set; } = new List<CriterionRequest>();

    }
}
