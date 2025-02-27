using KSMS.Domain.Dtos.Responses.ScoreDetailError;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.ErrorType
{
    public class ErrorTypeResponse
    {
        public Guid CriteriaId { get; set; }

        public string Name { get; set; } = null!;

       //  public virtual ICollection<ScoreDetailErrorResponse> ScoreDetailErrors { get; set; } = new List<ScoreDetailErrorResponse>();
    }
}
