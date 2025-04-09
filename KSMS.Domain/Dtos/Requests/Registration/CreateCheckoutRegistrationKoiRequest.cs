using System;
using System.ComponentModel.DataAnnotations;

namespace KSMS.Domain.Dtos.Requests.Registration;

public class CreateCheckoutRegistrationKoiRequest
{
    [Required(ErrorMessage = "Ảnh check-out là bắt buộc")]
    public required string ImgCheckOut { get; set; }
    
    public string? Notes { get; set; }
} 