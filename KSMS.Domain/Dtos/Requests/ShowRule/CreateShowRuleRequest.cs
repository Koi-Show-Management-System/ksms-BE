using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
using System.ComponentModel.DataAnnotations;

namespace KSMS.Domain.Dtos.Requests.ShowRule
{
    public class CreateShowRuleRequest
    {
        [Required(ErrorMessage = "ShowId is required.")]
        public Guid ShowId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title must not exceed 200 characters.")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Content is required.")]
        [StringLength(2000, ErrorMessage = "Content must not exceed 2000 characters.")]
        public string Content { get; set; } = null!;
    }
}
