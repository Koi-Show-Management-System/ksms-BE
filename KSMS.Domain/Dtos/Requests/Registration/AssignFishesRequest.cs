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

    public class UpdateFishTankRequest
    {
        public Guid RegistrationRoundId { get; set; }  // ID của bản ghi cá trong vòng thi
        public Guid TankId { get; set; }  // ID của hồ chứa cá
    }

    public class AssignToFirstRound
    {
        public Guid CategoryId { get; set; }
        public List<Guid> RegistrationIds { get; set; } = new List<Guid>();
    }
}
