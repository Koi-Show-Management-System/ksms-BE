using KSMS.Domain.Dtos.Responses.CompetitionCategory;
using KSMS.Domain.Dtos.Responses.Round;
using KSMS.Domain.Dtos.Responses.ShowRule;
using KSMS.Domain.Dtos.Responses.ShowStaff;
using KSMS.Domain.Dtos.Responses.ShowStatus;
using KSMS.Domain.Dtos.Responses.Sponsor;
using KSMS.Domain.Dtos.Responses.Ticket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace KSMS.Domain.Dtos.Responses.KoiShow
{
    public class KoiShowResponse
    {
        public Guid Id { get; set; }
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

        public virtual ICollection<CompetitionCategoryResponse> Categories { get; set; } = new List<CompetitionCategoryResponse>();

        public virtual ICollection<ShowStaffResponse> ShowStaffs { get; set; } = new List<ShowStaffResponse>();

        public virtual ICollection<ShowRuleResponse> ShowRules { get; set; } = new List<ShowRuleResponse>();

      //  public virtual ICollection<ShowStatisticResponse> ShowStatistics { get; set; } = new List<ShowStatisticResponse>();

        public virtual ICollection<ShowStatusResponse> ShowStatuses { get; set; } = new List<ShowStatusResponse>();

        public virtual ICollection<SponsorResponse> Sponsors { get; set; } = new List<SponsorResponse>();

        public virtual ICollection<TicketResponse> Tickets { get; set; } = new List<TicketResponse>();

        
    }
}
