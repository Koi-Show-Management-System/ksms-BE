using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.ShowRule
{
    public class UpdateShowRuleRequest
    {
        public Guid Id { get; set; }
        public Guid ShowId { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
    }
}
