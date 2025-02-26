using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.ShowStaff
{
    public class UpdateShowStaffRequest
    {
        public Guid Id { get; set; }
        
      //  public Guid ShowId { get; set; }

       
        public Guid AccountId { get; set; }

         
        public Guid AssignedBy { get; set; }

        
        public DateTime? AssignedAt { get; set; }
    }
}

