
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.Criterion
{
    public class UpdateCriteriaRequest
    {

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "If provided, Order must be greater than 0.")]
        public int? Order { get; set; }
    }
}
