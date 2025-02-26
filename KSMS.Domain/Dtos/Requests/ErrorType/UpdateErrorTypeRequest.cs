using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.ErrorType
{
    public class UpdateErrorTypeRequest
    {
           public Guid CriteriaId { get; set; }

        public string Name { get; set; } = null!;
    }
}
