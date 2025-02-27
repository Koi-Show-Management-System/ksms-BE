using KSMS.Domain.Dtos.Requests.ErrorType;
using KSMS.Domain.Dtos.Responses.ErrorType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.Criterion
{
    public class CriteriaResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public int? Order { get; set; }

        public  ICollection<ErrorTypeResponse> ErrorTypes { get; set; } = new List<ErrorTypeResponse>();

    }
}
