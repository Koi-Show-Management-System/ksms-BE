using KSMS.Domain.Dtos.Requests.RefereeAssignment;
using KSMS.Domain.Dtos.Requests.Round;
using KSMS.Domain.Dtos.Requests.Variety;
using KSMS.Domain.Dtos.Responses.Award;
using KSMS.Domain.Dtos.Responses.CategoryVariety;
using KSMS.Domain.Dtos.Responses.CriteriaGroupRequest;
using KSMS.Domain.Dtos.Responses.RefereeAssignment;
using KSMS.Domain.Dtos.Responses.Registration;
using KSMS.Domain.Dtos.Responses.Round;
using KSMS.Domain.Dtos.Responses.Variety;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.Categorie
{
    public class CompetitionCategoryResponse
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

        public virtual ICollection<RoundResponse> Rounds { get; set; } = new List<RoundResponse>();

        public virtual ICollection<AwardResponse> Awards { get; set; } = new List<AwardResponse>();

        public virtual ICollection<CategoryVarietyResponse> CategoryVarieties { get; set; } = new List<CategoryVarietyResponse>();

        public virtual ICollection<RegistrationStaffResponse> Registrations { get; set; } = new List<RegistrationStaffResponse>();


        public virtual ICollection<CriteriaGroupResponse> CriteriaGroups { get; set; } = new List<CriteriaGroupResponse>();

        public virtual ICollection<RefereeAssignmentResponse> RefereeAssignments { get; set; } = new List<RefereeAssignmentResponse>();
    }
}
