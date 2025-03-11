using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.Round
{
    public  class RoundGetResultRegistrationRoundResponse
    {
        public string? Name { get; set; }

        public int? RoundOrder { get; set; }

        public string RoundType { get; set; } = null!;

        public string? Status { get; set; }
    }
}
