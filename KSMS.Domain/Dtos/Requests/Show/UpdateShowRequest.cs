using KSMS.Domain.Dtos.Requests.Categorie;
using KSMS.Domain.Dtos.Requests.Round;
using KSMS.Domain.Dtos.Requests.ShowRule;
using KSMS.Domain.Dtos.Requests.ShowStaff;
using KSMS.Domain.Dtos.Requests.ShowStatus;
using KSMS.Domain.Dtos.Requests.Sponsor;
using KSMS.Domain.Dtos.Requests.Ticket;
using System.ComponentModel.DataAnnotations;

namespace KSMS.Domain.Dtos.Requests.Show
{
    public class UpdateShowRequest
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
        public virtual ICollection<UpdateCategorieShowRequest> Categories { get; set; } = new List<UpdateCategorieShowRequest>();
        public virtual ICollection<UpdateShowStaffRequest> ShowStaffs { get; set; } = new List<UpdateShowStaffRequest>();
        public virtual ICollection<UpdateShowRuleRequest> ShowRules { get; set; } = new List<UpdateShowRuleRequest>();
        public virtual ICollection<UpdateShowStatusRequest> ShowStatuses { get; set; } = new List<UpdateShowStatusRequest>();
        public virtual ICollection<UpdateSponsorRequest> Sponsors { get; set; } = new List<UpdateSponsorRequest>();
        public virtual ICollection<UpdateTicketRequest> Tickets { get; set; } = new List<UpdateTicketRequest>();
    }
}
