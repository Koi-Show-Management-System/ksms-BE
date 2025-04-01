using KSMS.Domain.Dtos.Responses.ShowRule;
using KSMS.Domain.Dtos.Responses.ShowStatus;
using KSMS.Domain.Dtos.Responses.Sponsor;
using KSMS.Domain.Dtos.Responses.TicketType;

namespace KSMS.Domain.Dtos.Responses.KoiShow;

public class GetKoiShowDetailResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime? StartExhibitionDate { get; set; }

    public DateTime? EndExhibitionDate { get; set; }

    public string? Location { get; set; }

    public string? Description { get; set; }
    
    public string? CancellationReason { get; set; }

    public DateOnly? RegistrationDeadline { get; set; }

    public int? MinParticipants { get; set; }

    public int? MaxParticipants { get; set; }

    public bool? HasGrandChampion { get; set; }

    public bool? HasBestInShow { get; set; }

    public string? ImgUrl { get; set; }

    public string? Status { get; set; }
    public DateTime? CreatedAt { get; set; }
        
    public DateTime? UpdatedAt { get; set; }

    public List<string> Criteria { get; set; } = [];

    public ICollection<RuleGetKoiShowDetailResponse> ShowRules { get; set; } = [];

    //public virtual ICollection<ShowStaff> ShowStaffs { get; set; } = new List<ShowStaff>();

    public ICollection<StatusGetKoiShowDetailResponse> ShowStatuses { get; set; } = [];

    public ICollection<SponsorGetKoiShowDetailResponse> Sponsors { get; set; } = [];

    public ICollection<TicketTypeGetKoiShowDetailResponse> TicketTypes { get; set; } = [];
}