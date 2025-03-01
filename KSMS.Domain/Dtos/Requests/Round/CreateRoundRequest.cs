
using System;
using System.ComponentModel.DataAnnotations;
using KSMS.Domain.Enums;

namespace KSMS.Domain.Dtos.Requests.Round
{
    public class CreateRoundRequest
    {
        [Required(ErrorMessage = "Name is required.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "RoundOrder is required.")]
        public int? RoundOrder { get; set; }

        [Required(ErrorMessage = "RoundType is required.")]
        public string RoundType { get; set; } = null!;
        
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        [Required(ErrorMessage = "MinScoreToAdvance is required.")]
        [Range(0, 100, ErrorMessage = "MinScoreToAdvance must be between 0 and 100.")]
        public decimal? MinScoreToAdvance { get; set; }

        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters.")]
        public string? Status { get; set; }
    }
}
