using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.Criterion
{
    public class CriterionResponse
    {
 
       
        public Guid? CriteriaGroupId { get; set; }

        public Guid Id { get; set; }


        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public decimal? MaxScore { get; set; }

        public decimal? Weight { get; set; }

        public int? Order { get; set; }
    }
}
