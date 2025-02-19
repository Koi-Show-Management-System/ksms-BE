using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.ShowStatus
{
    public class ShowStatusResponse
    {
        public Guid Id { get; set; }

        public Guid KoiShowId { get; set; }

        public string StatusName { get; set; } = null!;

        public string? Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }
    }
}
