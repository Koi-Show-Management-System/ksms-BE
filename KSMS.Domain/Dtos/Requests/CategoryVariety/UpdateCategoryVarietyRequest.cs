using KSMS.Domain.Dtos.Requests.Variety;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.CategoryVariety
{
    public class UpdateCategoryVarietyRequest
    {
     //   public Guid CompetitionCategoryId { get; set; }
        public Guid Id { get; set; }

        public Guid VarietyId { get; set; }

       // public virtual UpdateVarietyRequest UpdateVarietyRequests { get; set; }
    }
}
