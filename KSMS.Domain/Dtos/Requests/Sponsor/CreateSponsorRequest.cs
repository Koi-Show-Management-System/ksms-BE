using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.Sponsor
{
    public class CreateSponsorRequest
    { 
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(200, ErrorMessage = "Name must not exceed 200 characters.")]
        public string Name { get; set; } = null!;

        [Url(ErrorMessage = "LogoUrl must be a valid URL.")]
        public string? LogoUrl { get; set; }
        public decimal InvestMoney { get; set; }


    }
}
