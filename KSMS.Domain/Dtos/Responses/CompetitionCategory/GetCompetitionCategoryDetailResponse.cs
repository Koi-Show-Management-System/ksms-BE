using KSMS.Domain.Dtos.Responses.Award;
using KSMS.Domain.Dtos.Responses.CategoryVariety;
using KSMS.Domain.Dtos.Responses.CriteriaCompetitionCategory;
using KSMS.Domain.Dtos.Responses.RefereeAssignment;
using KSMS.Domain.Dtos.Responses.Round;

namespace KSMS.Domain.Dtos.Responses.CompetitionCategory;

public class GetCompetitionCategoryDetailResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal? SizeMin { get; set; }

    public decimal? SizeMax { get; set; }

    public string? Description { get; set; }

    public int? MaxEntries { get; set; }
    
    public bool? HasTank { get; set; }
    
    public decimal? RegistrationFee { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string? Status { get; set; }

    public ICollection<AwardGetCompetitionCategoryDetailResponse> Awards { get; set; } = [];

    public ICollection<GetCategoryVarietyResponse> CategoryVarieties { get; set; } = [];

    public ICollection<GetCriteriaCompetitionCategoryResponse> CriteriaCompetitionCategories { get; set; } = [];

    public ICollection<GetRefereeAssignmentResponse> RefereeAssignments { get; set; } = [];

    public ICollection<RoundGetCompetitionCategoryResponse> Rounds { get; set; } = [];
}