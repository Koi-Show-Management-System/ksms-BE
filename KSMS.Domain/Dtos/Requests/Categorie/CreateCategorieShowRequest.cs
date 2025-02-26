
using KSMS.Domain.Dtos.Requests.CategoryVariety;
using KSMS.Domain.Dtos.Requests.CriteriaCompetitionCategory;
using KSMS.Domain.Dtos.Requests.RefereeAssignment;
using KSMS.Domain.Dtos.Requests.Round;
using KSMS.Domain.Dtos.Requests.Variety;
using KSMS.Domain.Dtos.Responses.Award;
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
        
       // public Guid KoiShowId { get; set; } 
        public string Name { get; set; } = null!;

        
        public decimal? SizeMin { get; set; }

        
        public decimal? SizeMax { get; set; } 
         
        public string? Description { get; set; } 
        
        public int? MaxEntries { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

         
        public string? Status { get; set; }

        public  ICollection<CreateRoundRequest> Rounds { get; set; } = new List<CreateRoundRequest>();

        public  ICollection<CreateCategoryVarietyRequest> CategoryVarietys { get; set; } = new List<CreateCategoryVarietyRequest>();

        public  ICollection<CreateAwardCateShowRequest> Awards { get; set; } = new List<CreateAwardCateShowRequest>();

        public  ICollection<CreateCriteriaCompetitionCategoryRequest> CriteriaCompetitionCategories { get; set; } = new List<CreateCriteriaCompetitionCategoryRequest>();

        public  ICollection<CreateRefereeAssignmentRequest> RefereeAssignments { get; set; } = new List<CreateRefereeAssignmentRequest>();
    }

}
