
using KSMS.Domain.Dtos.Requests.CriteriaGroup;
using KSMS.Domain.Dtos.Requests.RefereeAssignment;
using KSMS.Domain.Dtos.Requests.Round;
using KSMS.Domain.Dtos.Requests.Variety;
using KSMS.Domain.Dtos.Responses.Award;
using KSMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.Categorie
{ 
    public class CreateCategorieShowRequest
    {
        
        public Guid ShowId { get; set; }

        
         
        public string Name { get; set; } = null!;

        
        public decimal? SizeMin { get; set; }

        
        public decimal? SizeMax { get; set; }

        public Guid? VarietyId { get; set; }

         
        public string? Description { get; set; }

        
        public int? MaxEntries { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

         
        public string? Status { get; set; }

        public virtual ICollection<CreateRoundRequest> Rounds { get; set; } = new List<CreateRoundRequest>();

        public virtual VarietyRequest? Variety { get; set; }

        public virtual ICollection<CreateAwardCateShowRequest> Awards { get; set; } = new List<CreateAwardCateShowRequest>();

        public virtual ICollection<CriteriaGroupRequest> CriteriaGroups { get; set; } = new List<CriteriaGroupRequest>();

        public virtual ICollection<RefereeAssignmentRequest> RefereeAssignments { get; set; } = new List<RefereeAssignmentRequest>();
    }

}
