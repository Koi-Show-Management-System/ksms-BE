using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.ShowStatistic
{
    public class ShowStatisticResponse
    {
        public Guid Id { get; set; }

        public Guid? ShowId { get; set; }

        public string MetricName { get; set; } = null!;

        public decimal MetricValue { get; set; }
    }
}
