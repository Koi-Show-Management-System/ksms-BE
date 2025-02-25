
using System;
using System.ComponentModel.DataAnnotations;
using KSMS.Domain.Enums;

namespace KSMS.Domain.Dtos.Requests.Round
{
    public class CreateRoundRequest
    {
        //[Required(ErrorMessage = "CategoryId is required.")]
        //public Guid CompetitionCategoryId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "RoundOrder is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "RoundOrder must be greater than 0.")]
        public int? RoundOrder { get; set; }

        [Required(ErrorMessage = "RoundType is required.")]
        [StringLength(50, ErrorMessage = "RoundType cannot exceed 50 characters.")]
        //[EnumDataType(typeof(RoleName))]
        public string RoundType { get; set; } = null!;

        [Required(ErrorMessage = "StartTime is required.")]
        public DateTime? StartTime { get; set; }

        [Required(ErrorMessage = "EndTime is required.")]
        [DateGreaterThan(nameof(StartTime), ErrorMessage = "EndTime must be later than StartTime.")]
        public DateTime? EndTime { get; set; }

        [Required(ErrorMessage = "MinScoreToAdvance is required.")]
        [Range(0, 100, ErrorMessage = "MinScoreToAdvance must be between 0 and 100.")]
        public decimal? MinScoreToAdvance { get; set; }

        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters.")]
        public string? Status { get; set; }
    }

    // Custom validation attribute for comparing dates
    public class DateGreaterThanAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public DateGreaterThanAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var currentValue = value as DateTime?;
            var comparisonProperty = validationContext.ObjectType.GetProperty(_comparisonProperty);

            if (comparisonProperty == null)
            {
                return new ValidationResult($"Property '{_comparisonProperty}' not found.");
            }

            var comparisonValue = comparisonProperty.GetValue(validationContext.ObjectInstance) as DateTime?;

            if (currentValue.HasValue && comparisonValue.HasValue && currentValue <= comparisonValue)
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
