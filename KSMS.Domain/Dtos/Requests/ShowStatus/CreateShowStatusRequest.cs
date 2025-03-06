using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.ShowStatus
{
    public class CreateShowStatusRequest
    {
        [Required(ErrorMessage = "StatusName is required.")]
        [StringLength(50, ErrorMessage = "StatusName must not exceed 50 characters.")]
        public string StatusName { get; set; } = null!;

        [StringLength(255, ErrorMessage = "Description must not exceed 255 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "StartDate is required.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "EndDate is required.")]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }
    }
}
