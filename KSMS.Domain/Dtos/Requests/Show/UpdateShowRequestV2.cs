using KSMS.Domain.Dtos.Requests.ShowRule;
using KSMS.Domain.Dtos.Requests.ShowStatus;
using KSMS.Domain.Dtos.Requests.Sponsor;
using KSMS.Domain.Dtos.Requests.Ticket;

namespace KSMS.Domain.Dtos.Requests.Show;

public class UpdateShowRequestV2
{
    public string Name { get; set; } = null!;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? StartExhibitionDate { get; set; }
    public DateTime? EndExhibitionDate { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public DateOnly? RegistrationDeadline { get; set; }
    public int? MinParticipants { get; set; }
    public int? MaxParticipants { get; set; }
    public bool? HasGrandChampion { get; set; }
    public bool? HasBestInShow { get; set; }
    public string? ImgUrl { get; set; }
    public decimal RegistrationFee { get; set; }
    public string? Status { get; set; }
    // public List<Guid> AssignStaffRequests { get; set; } = [];
    // public List<Guid> AssignManagerRequests { get; set; } = [];
    // public List<UpdateShowRuleRequestV2> UpdateShowRuleRequests { get; set; } = [];
    // public List<UpdateShowStatusRequestV2> UpdateShowStatusRequests { get; set; } = [];
    // public List<UpdateSponsorRequestV2> UpdateSponsorRequests { get; set; } = [];
    // public List<UpdateTicketRequestV2> UpdateTicketRequests { get; set; } = [];
}