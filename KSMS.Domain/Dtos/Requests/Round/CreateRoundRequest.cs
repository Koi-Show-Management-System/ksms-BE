
using System;
using System.ComponentModel.DataAnnotations;
using KSMS.Domain.Enums;

namespace KSMS.Domain.Dtos.Requests.Round
{
    public class CreateRoundRequest
    {
        [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters.")]
        public string? Name { get; set; }

        public int? RoundOrder { get; set; }

        [Required(ErrorMessage = "RoundType is required.")]
        [StringLength(20, ErrorMessage = "RoundType cannot exceed 20 characters.")]
        public string RoundType { get; set; } = null!;
        
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        [Range(0, 100, ErrorMessage = "MinScoreToAdvance must be between 0 and 100.")]
        public decimal? MinScoreToAdvance { get; set; }

        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters.")]
        public string? Status { get; set; }
    }
}
