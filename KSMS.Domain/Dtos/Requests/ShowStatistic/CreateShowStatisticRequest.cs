using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
using System.ComponentModel.DataAnnotations;

namespace KSMS.Domain.Dtos.Requests.ShowStatistic
{
    public class CreateShowStatisticRequest
    {
        

        [Required(ErrorMessage = "ShowId is required.")]
        public Guid? ShowId { get; set; }

        [Required(ErrorMessage = "MetricName is required.")]
        [StringLength(200, ErrorMessage = "MetricName must not exceed 200 characters.")]
        public string MetricName { get; set; } = null!;

        [Required(ErrorMessage = "MetricValue is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "MetricValue must be a positive number.")]
        public decimal MetricValue { get; set; }
    }
}
