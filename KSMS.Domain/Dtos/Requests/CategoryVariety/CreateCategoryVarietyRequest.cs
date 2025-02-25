using KSMS.Domain.Dtos.Requests.Variety;
using KSMS.Domain.Dtos.Responses.Variety;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.CategoryVariety
{
    public class CreateCategoryVarietyRequest

    {
      //  public Guid CompetitionCategoryId { get; set; }
     //   public Guid Id { get; set; }

        public Guid VarietyId { get; set; } 

        public virtual CreateVarietyRequest Variety { get; set; }
    }
}
