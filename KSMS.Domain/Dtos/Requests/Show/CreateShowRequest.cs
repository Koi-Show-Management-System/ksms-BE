using KSMS.Domain.Dtos.Requests.Categorie;
using KSMS.Domain.Dtos.Requests.Round;
using KSMS.Domain.Dtos.Requests.ShowRule;
using KSMS.Domain.Dtos.Requests.ShowStaff;
using KSMS.Domain.Dtos.Requests.ShowStatistic;
using KSMS.Domain.Dtos.Requests.ShowStatus;
using KSMS.Domain.Dtos.Requests.Sponsor;
using KSMS.Domain.Dtos.Requests.Ticket;
using System.ComponentModel.DataAnnotations;

namespace KSMS.Domain.Dtos.Requests.Show
{
    public class CreateShowRequest
    {
        // Tên không được null và giới hạn độ dài
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
        public string Name { get; set; } = null!;

        // Ngày bắt đầu không được null
        [Required(ErrorMessage = "StartDate is required.")]
        public DateTime? StartDate { get; set; }

        // Ngày kết thúc phải lớn hơn ngày bắt đầu
        [Required(ErrorMessage = "EndDate is required.")]
        [DateGreaterThan(nameof(StartDate), ErrorMessage = "EndDate must be later than StartDate.")]
        public DateTime? EndDate { get; set; }

        // Ngày triển lãm bắt đầu và kết thúc (không bắt buộc)
        public DateTime? StartExhibitionDate { get; set; }
        public DateTime? EndExhibitionDate { get; set; }

        // Địa điểm có giới hạn độ dài
        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters.")]
        public string? Location { get; set; }

        // Mô tả (không giới hạn kích thước)
        public string? Description { get; set; }

        // Deadline đăng ký
        public DateOnly? RegistrationDeadline { get; set; }

        // Số người tham gia tối thiểu và tối đa
        [Range(0, int.MaxValue, ErrorMessage = "MinParticipants must be a non-negative number.")]
        public int? MinParticipants { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "MaxParticipants must be a non-negative number.")]
        public int? MaxParticipants { get; set; }

        // Có giải Grand Champion và Best in Show
        public bool? HasGrandChampion { get; set; }
        public bool? HasBestInShow { get; set; }

        // Ảnh URL không vượt quá 255 ký tự
        [StringLength(255, ErrorMessage = "ImgUrl cannot exceed 255 characters.")]
        public string? ImgUrl { get; set; }

        // Phí đăng ký
        [Range(0, double.MaxValue, ErrorMessage = "RegistrationFee must be a non-negative value.")]
        public decimal RegistrationFee { get; set; }

        // Trạng thái không vượt quá 20 ký tự
        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters.")]
        public string? Status { get; set; }

        // Các danh sách liên quan
        public virtual ICollection<CreateCategorieShowRequest> Categories { get; set; } = new List<CreateCategorieShowRequest>();
        public virtual ICollection<CreateShowStaffRequest> ShowStaffs { get; set; } = new List<CreateShowStaffRequest>();
        public virtual ICollection<CreateShowRuleRequest> ShowRules { get; set; } = new List<CreateShowRuleRequest>();
        public virtual ICollection<CreateShowStatisticRequest> ShowStatistics { get; set; } = new List<CreateShowStatisticRequest>();
        public virtual ICollection<CreateShowStatusRequest> ShowStatuses { get; set; } = new List<CreateShowStatusRequest>();
        public virtual ICollection<CreateSponsorRequest> Sponsors { get; set; } = new List<CreateSponsorRequest>();
        public virtual ICollection<CreateTicketRequest> Tickets { get; set; } = new List<CreateTicketRequest>();

       
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
