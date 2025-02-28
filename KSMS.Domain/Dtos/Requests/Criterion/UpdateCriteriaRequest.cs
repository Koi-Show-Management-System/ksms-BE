
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

        public string Name { get; set; } = null!; 
 
        public string? Description { get; set; } 
            
       
        public int? Order { get; set; }
    }
}
