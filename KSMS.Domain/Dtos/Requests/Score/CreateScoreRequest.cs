using System;
using System.ComponentModel.DataAnnotations;

namespace KSMS.Domain.Dtos.Requests.Score
{
    public class CreateScoreRequest
    {
        [Required(ErrorMessage = "RegistrationId is required.")]
        public Guid RegistrationId { get; set; }

        [Required(ErrorMessage = "RoundId is required.")]
        public Guid RoundId { get; set; }

        [Required(ErrorMessage = "RefereeAccountId is required.")]
        public Guid RefereeAccountId { get; set; }

        [Required(ErrorMessage = "CriteriaId is required.")]
        public Guid CriteriaId { get; set; }

       
        public decimal? Score1 { get; set; }

        [StringLength(500, ErrorMessage = "Comments cannot exceed 500 characters.")]
        public string? Comments { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [RegularExpression("^(pending|approved|rejected)$", ErrorMessage = "Status must be 'pending', 'approved', or 'rejected'.")]
        public string Status { get; set; } = "pending";
    }
}
