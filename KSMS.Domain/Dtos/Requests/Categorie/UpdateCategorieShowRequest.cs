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
        public virtual ICollection<UpdateRoundRequest> Rounds { get; set; } = new List<UpdateRoundRequest>();
        public virtual ICollection<UpdateCategoryVarietyRequest> CategoryVarietys { get; set; } = new List<UpdateCategoryVarietyRequest>();
        public virtual ICollection<UpdateAwardCateShowRequest> Awards { get; set; } = new List<UpdateAwardCateShowRequest>();
        public virtual ICollection<UpdateCriteriaCompetitionCategoryRequest> CriteriaGroups { get; set; } = new List<UpdateCriteriaCompetitionCategoryRequest>();
        public virtual ICollection<UpdateRefereeAssignmentRequest> RefereeAssignments { get; set; } = new List<UpdateRefereeAssignmentRequest>();
    }
}
