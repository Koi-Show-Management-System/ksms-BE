using KSMS.Domain.Dtos.Responses.Round;
using KSMS.Domain.Dtos.Responses.RoundResult;
using KSMS.Domain.Dtos.Responses.Score;
using KSMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.RegistrationRound
{
   public class GetResultRegistrationRoundResponse
    {
        public virtual RoundGetResultRegistrationRoundResponse Round { get; set; } = null!;

        public virtual ICollection<GetRoundResultResponse> RoundResults { get; set; } = new List<GetRoundResultResponse>();

        public virtual ICollection<GetScoreDetailResponse> ScoreDetails { get; set; } = new List<GetScoreDetailResponse>();
    }
}
