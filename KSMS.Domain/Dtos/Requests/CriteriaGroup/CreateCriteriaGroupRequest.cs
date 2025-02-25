using KSMS.Domain.Dtos.Requests.Criterion;
using KSMS.Domain.Dtos.Requests.ErrorType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.CriteriaGroup
{
    public class CreateCriteriaGroupRequest
    {
      //  public Guid Id {  get; set; }
    //    public Guid CompetitionCategoryId { get; set; }

        public string Name { get; set; } = null!;   

        public Guid CriteriaId { get; set; }

        public string? RoundType { get; set; }

        public string? Description { get; set; }

        public CreateCriterionRequest Criterias { get; set; } = null!;



    }
}
