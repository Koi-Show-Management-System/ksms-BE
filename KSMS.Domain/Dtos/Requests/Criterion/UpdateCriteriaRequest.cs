using KSMS.Domain.Dtos.Requests.ErrorType;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.Criterion
{
    public class UpdateCriteriaRequest
    {
     //   public Guid Id { get; set; }
   
     //   public Guid? CriteriaGroupId { get; set; }

        public string Name { get; set; } = null!; 
 
        public string? Description { get; set; } 
            
       
        public int? Order { get; set; }

        public virtual ICollection<UpdateErrorTypeRequest> UpdateErrorTypeRequests { get; set; } = new List<UpdateErrorTypeRequest>();
    }
}
