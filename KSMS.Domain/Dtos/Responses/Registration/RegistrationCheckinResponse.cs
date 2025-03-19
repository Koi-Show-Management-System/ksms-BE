using KSMS.Domain.Dtos.Responses.KoiMedium;
using KSMS.Domain.Dtos.Responses.KoiProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.Registration
{
    public class RegistrationCheckinResponse
    {
        public Guid Id { get; set; }
        public string? RegistrationNumber { get; set; }

        public string RegisterName { get; set; } = null!;

        public decimal KoiSize { get; set; }

        public int KoiAge { get; set; } 
 
        public string? Status { get; set; }

        public string? Notes { get; set; }
        
        public DateTime? ApprovedAt { get; set; }

        public DateTime? CheckInExpiredDate { get; set; }

        public bool? IsCheckedIn { get; set; }

        public DateTime? CheckInTime { get; set; }

        public string? CheckInLocation { get; set; }

        public Guid? CheckedInBy { get; set; }
        
        public CompetitionCategoryCheckinResponse? CompetitionCategory { get; set; }

        public virtual KoiProfileCheckinResponse KoiProfile { get; set; } = null!;

        public ICollection<GetKoiMediaResponse> KoiMedia { get; set; } = new List<GetKoiMediaResponse>();
        
    }
}
