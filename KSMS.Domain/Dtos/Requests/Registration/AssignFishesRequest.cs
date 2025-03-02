using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.Registration
{
    public class AssignFishesRequest
    {
        public Guid RoundId { get; set; }
        public List<Guid> RegistrationIds { get; set; } = new List<Guid>();
    }

}
