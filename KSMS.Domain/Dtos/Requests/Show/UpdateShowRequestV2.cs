using System.ComponentModel.DataAnnotations;
using KSMS.Domain.Dtos.Requests.ShowRule;
using KSMS.Domain.Dtos.Requests.ShowStatus;
using KSMS.Domain.Dtos.Requests.Sponsor;
using KSMS.Domain.Dtos.Requests.Ticket;

namespace KSMS.Domain.Dtos.Requests.Show;

public class UpdateShowRequestV2
{
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
    public string? Name { get; set; } = null!;

    
    public DateTime? StartDate { get; set; }

    
    //[DateGreaterThan(nameof(StartDate), ErrorMessage = "EndDate must be later than StartDate.")]
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

       
    [Range(0, double.MaxValue, ErrorMessage = "RegistrationFee must be a non-negative value.")]
    public decimal RegistrationFee { get; set; }

         
    [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters.")]
    public string? Status { get; set; }
    
}