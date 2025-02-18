using KSMS.Domain.Dtos.Responses.Variety;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.CategoryVariety
{
    public class CategoryVarietyResponse
    {
        public Guid Id { get; set; }

        public Guid VarietyId { get; set; }

        public Guid CompetitionCategoryId { get; set; }

        public virtual VarietyResponse Variety { get; set; }

    }
}
