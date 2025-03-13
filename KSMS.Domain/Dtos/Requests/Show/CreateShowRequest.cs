using KSMS.Domain.Dtos.Requests.Categorie;
using KSMS.Domain.Dtos.Requests.Round;
using KSMS.Domain.Dtos.Requests.ShowRule;
using KSMS.Domain.Dtos.Requests.ShowStaff;
using KSMS.Domain.Dtos.Requests.ShowStatus;
using KSMS.Domain.Dtos.Requests.Sponsor;
using KSMS.Domain.Dtos.Requests.Ticket;
using KSMS.Domain.Dtos.Requests.TicketType;
using System.ComponentModel.DataAnnotations;

namespace KSMS.Domain.Dtos.Requests.Show
{
    public class CreateShowRequest
    {
         
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
        public string Name { get; set; } = null!;

       
        [Required(ErrorMessage = "StartDate is required.")]
        public DateTime? StartDate { get; set; }

        
        [Required(ErrorMessage = "EndDate is required.")]
        [DateGreaterThan(nameof(StartDate), ErrorMessage = "EndDate must be later than StartDate.")]
        public DateTime? EndDate { get; set; }

        
        public DateTime? StartExhibitionDate { get; set; }
        public DateTime? EndExhibitionDate { get; set; }
 
        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters.")]
        public string? Location { get; set; }

         
        public string? Description { get; set; }
 
        public DateOnly? RegistrationDeadline { get; set; }

        
        [Range(0, int.MaxValue, ErrorMessage = "MinParticipants must be a non-negative number.")]
        public int? MinParticipants { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "MaxParticipants must be a non-negative number.")]
        public int? MaxParticipants { get; set; }

        
        public bool? HasGrandChampion { get; set; }
        public bool? HasBestInShow { get; set; }

       
        [StringLength(255, ErrorMessage = "ImgUrl cannot exceed 255 characters.")]
        public string? ImgUrl { get; set; }
        

         
        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters.")]
        public string? Status { get; set; }

        
        public  ICollection<CreateCategorieShowRequest> CreateCategorieShowRequests { get; set; } = [];
        public  ICollection<Guid> AssignStaffRequests { get; set; } = [];
        public  ICollection<Guid> AssignManagerRequests { get; set; } = [];
        public  ICollection<CreateShowRuleRequest> CreateShowRuleRequests { get; set; } = []; 
        public  ICollection<CreateShowStatusRequest> CreateShowStatusRequests { get; set; } = [];
        public  ICollection<CreateSponsorRequest> CreateSponsorRequests { get; set; } = [];
        public  ICollection<CreateTicketTypeRequest> CreateTicketTypeRequests { get; set; } = [];
    }

     
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
