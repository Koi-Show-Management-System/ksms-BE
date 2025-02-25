using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.RoundResult
{
    public class CreateRoundResult
    {
        public Guid RegistrationRoundsId { get; set; }

        public decimal TotalScore { get; set; }

        public bool? IsPublic { get; set; }

        public string? Comments { get; set; }

        public string? Status { get; set; }
    }
}
