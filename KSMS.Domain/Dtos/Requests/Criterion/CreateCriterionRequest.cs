using KSMS.Domain.Dtos.Requests.CriteriaCompetitionCategory;
using KSMS.Domain.Dtos.Requests.ErrorType;
using System;
using System.ComponentModel.DataAnnotations;

namespace KSMS.Domain.Dtos.Requests.Criterion
{
    public class CreateCriterionRequest
    {
      
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

       

        [Required(ErrorMessage = "Order is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Order must be greater than 0.")]
        public int? Order { get; set; }

      //  public virtual ICollection<CreateErrorTypeRequest> ErrorTypes { get; set; } = new List<CreateErrorTypeRequest>();

   
    }
}
