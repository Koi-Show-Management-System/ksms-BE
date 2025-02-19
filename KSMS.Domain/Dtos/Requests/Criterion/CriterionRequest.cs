using System;
using System.ComponentModel.DataAnnotations;

namespace KSMS.Domain.Dtos.Requests.Criterion
{
    public class CriterionRequest
    {
      
        public Guid? CriteriaGroupId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "MaxScore is required.")]
        [Range(0, 100, ErrorMessage = "MaxScore must be between 0 and 100.")]
        public decimal? MaxScore { get; set; }

        [Required(ErrorMessage = "Weight is required.")]
        [Range(0, 1, ErrorMessage = "Weight must be between 0 and 1.")]
        public decimal? Weight { get; set; }

        [Required(ErrorMessage = "Order is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Order must be greater than 0.")]
        public int? Order { get; set; }
    }
}
