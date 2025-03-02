
using KSMS.Domain.Dtos.Requests.CategoryVariety;
using KSMS.Domain.Dtos.Requests.CriteriaCompetitionCategory;
using KSMS.Domain.Dtos.Requests.RefereeAssignment;
using KSMS.Domain.Dtos.Requests.Round;
using KSMS.Domain.Dtos.Requests.Variety;

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
        public string Name { get; set; } = null!;
        
        
        public decimal? SizeMin { get; set; }

        
        public decimal? SizeMax { get; set; } 
         
        public string? Description { get; set; } 
        
        public int? MaxEntries { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

         
        public string? Status { get; set; }

        public  ICollection<CreateRoundRequest> CreateRoundRequests { get; set; } = [];

        public List<Guid> CreateCompetionCategoryVarieties { get; set; } = [];

        public  ICollection<CreateAwardCateShowRequest> CreateAwardCateShowRequests { get; set; } = [];

        public  ICollection<CreateCriteriaCompetitionCategoryRequest> CreateCriteriaCompetitionCategoryRequests { get; set; } = [];

        public  ICollection<CreateRefereeAssignmentRequest> CreateRefereeAssignmentRequests { get; set; } = [];
    }

}
