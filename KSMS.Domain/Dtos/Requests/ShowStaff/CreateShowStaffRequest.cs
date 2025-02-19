using KSMS.Domain.Dtos.Requests.Account;
using KSMS.Domain.Dtos.Responses.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
using System.ComponentModel.DataAnnotations;

namespace KSMS.Domain.Dtos.Requests.ShowStaff
{
    public class CreateShowStaffRequest
    {
        

        //[Required(ErrorMessage = "ShowId is required.")]
        //public Guid KoiShowId { get; set; }

        [Required(ErrorMessage = "AccountId is required.")]
        public Guid AccountId { get; set; }

        [Required(ErrorMessage = "AssignedBy is required.")]
        public Guid AssignedBy { get; set; }

        [Required(ErrorMessage = "AssignedAt is required.")]
        public DateTime AssignedAt { get; set; }
    }
}

