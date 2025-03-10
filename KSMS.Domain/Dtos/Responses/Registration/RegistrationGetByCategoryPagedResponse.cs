using KSMS.Domain.Dtos.Responses.CompetitionCategory;
using KSMS.Domain.Dtos.Responses.KoiMedium;
using KSMS.Domain.Dtos.Responses.RegistrationRound;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.Registration
{
    public class RegistrationGetByCategoryPagedResponse
    {
         
        public string? RegistrationNumber { get; set; }

        public decimal KoiSize { get; set; }

        public int KoiAge { get; set; } 
    
        public virtual ICollection<GetKoiMediaResponse> KoiMedia { get; set; } = new List<GetKoiMediaResponse>();

        public virtual ICollection<GetResultRegistrationRoundResponse> RegistrationRounds { get; set; } = new List<GetResultRegistrationRoundResponse>();
    }
}
