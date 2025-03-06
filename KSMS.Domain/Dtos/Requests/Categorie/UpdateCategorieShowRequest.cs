using System.ComponentModel.DataAnnotations;
using KSMS.Domain.Dtos.Requests.Award;
using KSMS.Domain.Dtos.Requests.CategoryVariety;
using KSMS.Domain.Dtos.Requests.CriteriaCompetitionCategory;
using KSMS.Domain.Dtos.Requests.RefereeAssignment;
using KSMS.Domain.Dtos.Requests.Round;
using KSMS.Domain.Dtos.Requests.Variety;

namespace KSMS.Domain.Dtos.Requests.Categorie
{
    public class UpdateCategorieShowRequest
    {
        public Guid Id { get; set; }
      //  public Guid ShowId { get; set; }
        public string Name { get; set; } = null!;
        [Range(0, 999.99, ErrorMessage = "SizeMin must be between 0 and 999.99.")]
        public decimal? SizeMin { get; set; }
        [Range(0, 999.99, ErrorMessage = "SizeMax must be between 0 and 999.99.")]
        public decimal? SizeMax { get; set; }
        public Guid? VarietyId { get; set; }
        public string? Description { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "MaxEntries must be greater than 0.")]
        public int? MaxEntries { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Status { get; set; }
        public virtual ICollection<UpdateRoundRequest> UpdateRoundRequest { get; set; } = new List<UpdateRoundRequest>();
        public virtual ICollection<UpdateCategoryVarietyRequest> UpdateCategoryVarietyRequests { get; set; } = new List<UpdateCategoryVarietyRequest>();
        public virtual ICollection<UpdateAwardCateShowRequest> UpdateAwardCateShowRequests { get; set; } = new List<UpdateAwardCateShowRequest>();
        public virtual ICollection<UpdateCriteriaCompetitionCategoryRequest> UpdateCriteriaCompetitionCategoryRequests { get; set; } = new List<UpdateCriteriaCompetitionCategoryRequest>();
        public virtual ICollection<UpdateRefereeAssignmentRequest> UpdateRefereeAssignmentRequests { get; set; } = new List<UpdateRefereeAssignmentRequest>();
    }
}
