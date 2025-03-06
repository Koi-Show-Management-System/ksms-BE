using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace KSMS.Domain.Dtos.Requests.KoiProfile;

public class CreateKoiProfileRequest
{
    [Required(ErrorMessage = "Variety ID is required.")]
    public Guid VarietyId { get; set; }
    
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Name { get; set; } = null!;
    
    [Required(ErrorMessage = "Size is required.")]
    [Range(0.01, 999.99, ErrorMessage = "Size must be between 0.01 and 999.99.")]
    public decimal Size { get; set; }
    
    [Required(ErrorMessage = "Age is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Age must be greater than 0.")]
    public int Age { get; set; }
    
    [Required(ErrorMessage = "Gender is required.")]
    [StringLength(10, ErrorMessage = "Gender cannot exceed 10 characters.")]
    public string Gender { get; set; } = null!;
    
    [Required(ErrorMessage = "Bloodline is required.")]
    [StringLength(100, ErrorMessage = "Bloodline cannot exceed 100 characters.")]
    public string Bloodline { get; set; } = null!;
    
    [Required(ErrorMessage = "Status is required.")]
    [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters.")]
    public string Status { get; set; } = null!;
    public List<IFormFile> KoiImages { get; set; } = [];
    public List<IFormFile> KoiVideos { get; set; } = [];
}