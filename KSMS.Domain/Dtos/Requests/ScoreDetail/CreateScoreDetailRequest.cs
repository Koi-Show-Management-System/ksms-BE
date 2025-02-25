using KSMS.Domain.Dtos.Requests.Round;
using KSMS.Domain.Dtos.Requests.ScoreDetailError;
using System;
using System.ComponentModel.DataAnnotations;

namespace KSMS.Domain.Dtos.Requests.Score
{
    public class CreateScoreDetailRequest
    {
        public Guid RefereeAccountId { get; set; }
        public Guid RegistrationRoundId { get; set; } 
        public decimal InitialScore { get; set; }

        public decimal TotalPointMinus { get; set; }

        public bool? IsPublic { get; set; }

        public string? Comments { get; set; }


        public virtual ICollection<CreateScoreDetailErrorRequest> CreateScoreDetailErrors { get; set; } = new List<CreateScoreDetailErrorRequest>();

    }
}
