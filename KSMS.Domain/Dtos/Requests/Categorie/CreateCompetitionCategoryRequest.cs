using System.ComponentModel.DataAnnotations;
using KSMS.Domain.Dtos.Requests.CriteriaCompetitionCategory;
using KSMS.Domain.Dtos.Requests.RefereeAssignment;
using KSMS.Domain.Dtos.Requests.Round;

namespace KSMS.Domain.Dtos.Requests.Categorie;

public class CreateCompetitionCategoryRequest
{
    [Required(ErrorMessage = "Category name is required.")]
    [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters.")]
    public string Name { get; set; } = null!;
        
    public Guid KoiShowId { get; set; }
    [Range(0, 999.99, ErrorMessage = "Minimum size must be between 0 and 999.99.")]
    public decimal? SizeMin { get; set; }

    [Range(0, 999.99, ErrorMessage = "Maximum size must be between 0 and 999.99.")]
    public decimal? SizeMax { get; set; }

    public string? Description { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Maximum entries must be greater than 0.")]
    public int? MaxEntries { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters.")]
    public string? Status { get; set; }

    public  ICollection<CreateRoundRequest> CreateRoundRequests { get; set; } = [];

    public List<Guid> CreateCompetionCategoryVarieties { get; set; } = [];

    public  ICollection<CreateAwardCateShowRequest> CreateAwardCateShowRequests { get; set; } = [];

    public  ICollection<CreateCriteriaCompetitionCategoryRequest> CreateCriteriaCompetitionCategoryRequests { get; set; } = [];

    public  ICollection<CreateRefereeAssignmentRequest> CreateRefereeAssignmentRequests { get; set; } = [];
}