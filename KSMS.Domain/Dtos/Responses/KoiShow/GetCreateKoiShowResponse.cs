using KSMS.Domain.Dtos.Responses.Account;
using KSMS.Domain.Dtos.Responses.CategoryVariety;
using KSMS.Domain.Dtos.Responses.Criterion;
using KSMS.Domain.Dtos.Responses.ShowStaff;
using KSMS.Domain.Dtos.Responses.Variety;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.KoiShow
{
    public class GetCreateKoiShowResponse
    {
        public virtual ICollection<GetALLAccountResponse> Staffs { get; set; } = new List<GetALLAccountResponse>();
        public virtual ICollection<VarietyResponse> Varietys { get; set; } = new List<VarietyResponse>();
        public virtual ICollection<GetAllCriterionResponse> Critions { get; set; } = new List<GetAllCriterionResponse>();

    }
}
