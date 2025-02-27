using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.ShowRule
{
    public class GetAllShowRuleResponse
    {
        public Guid Id { get; set; }
        public Guid KoiShowId { get; set; }

        public string Title { get; set; } = null!;

        public string Content { get; set; } = null!;

    }
}
