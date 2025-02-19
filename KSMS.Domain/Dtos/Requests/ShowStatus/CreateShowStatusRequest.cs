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
       

      //  [Required(ErrorMessage = "ShowId is required.")]
      ////  public Guid KoiShowId { get; set; }

        [Required(ErrorMessage = "StatusName is required.")]
        [StringLength(100, ErrorMessage = "StatusName must not exceed 100 characters.")]
        public string StatusName { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Description must not exceed 500 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "StartDate is required.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "EndDate is required.")]
       // [DateGreaterThan(nameof(StartDate), ErrorMessage = "EndDate must be later than StartDate.")]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }
    }
}
