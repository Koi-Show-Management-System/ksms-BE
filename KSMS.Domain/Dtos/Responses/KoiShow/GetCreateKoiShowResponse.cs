using KSMS.Domain.Dtos.Responses.Account;
using KSMS.Domain.Dtos.Responses.CategoryVariety;
using KSMS.Domain.Dtos.Responses.CriteriaGroupRequest;
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
        public virtual ICollection<AccountResponse> Staffs { get; set; } = new List<AccountResponse>();
        public virtual ICollection<VarietyResponse> Varietys { get; set; } = new List<VarietyResponse>();
        public virtual ICollection<GetAllCriteriaGroupResponse> Critions { get; set; } = new List<GetAllCriteriaGroupResponse>();

    }
}
