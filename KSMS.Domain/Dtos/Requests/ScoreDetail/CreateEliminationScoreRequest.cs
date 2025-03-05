using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.ScoreDetail
{
    public class CreateEliminationScoreRequest
    {
        public Guid RefereeAccountId { get; set; }
        public Guid RegistrationRoundId { get; set; }
        public bool IsPass { get; set; }
    }

}
