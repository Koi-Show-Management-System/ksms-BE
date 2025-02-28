using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.Tank
{
    public class UpdateTankRequest
    {
        public string Name { get; set; } = null!;
        public int Capacity { get; set; }
        public string? WaterType { get; set; }
        public decimal? Temperature { get; set; }
        public decimal? Phlevel { get; set; }
        public decimal? Size { get; set; }
        public string? Location { get; set; }
        public string? Status { get; set; }
    }
}
