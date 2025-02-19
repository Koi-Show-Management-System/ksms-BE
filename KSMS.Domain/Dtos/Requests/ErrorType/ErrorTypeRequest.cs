using KSMS.Domain.Dtos.Requests.ScoreDetailError;
using KSMS.Domain.Dtos.Responses.ScoreDetailError;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.ErrorType
{
    public class ErrorTypeRequest
    {
        public Guid CriteriaId { get; set; }

        public string Name { get; set; } = null!;

        // public virtual ICollection<ScoreDetailErrorRequest> ScoreDetailErrors { get; set; } = new List<ScoreDetailErrorRequest>();
    }
}
