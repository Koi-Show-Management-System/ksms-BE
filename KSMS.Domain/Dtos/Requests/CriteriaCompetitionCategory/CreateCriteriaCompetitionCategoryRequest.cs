using KSMS.Domain.Dtos.Requests.Criterion;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.CriteriaCompetitionCategory
{
    public class CreateCriteriaCompetitionCategoryRequest
    { 
        public Guid CriteriaId { get; set; }

        [StringLength(20, ErrorMessage = "Round type cannot exceed 20 characters.")]
        public string? RoundType { get; set; }

        [Range(0.01, 9.99, ErrorMessage = "Weight must be between 0.01 and 9.99.")]
        public decimal? Weight { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Order must be greater than 0.")]
        public int? Order { get; set; }
    }
}
