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
        [StringLength(255, ErrorMessage = "Name must not exceed 255 characters.")]
        public string Name { get; set; } = null!;

        [StringLength(500, ErrorMessage = "LogoUrl must not exceed 500 characters.")]
        [Url(ErrorMessage = "LogoUrl must be a valid URL.")]
        public string? LogoUrl { get; set; }

        [Required(ErrorMessage = "InvestMoney is required.")]
        [Range(0.01, 9999999999999999.99, ErrorMessage = "InvestMoney must be positive and within decimal(18,2) range.")]
        public decimal InvestMoney { get; set; }


    }
}
