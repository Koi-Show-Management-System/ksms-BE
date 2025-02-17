namespace KSMS.Domain.Dtos.Responses.Registration;

public class CheckOutRegistrationResponse
{
    public required string Message { get; set; }
    public required string Url { get; set; }
}
 
public class RegistrationStaffResponse
{
   
    public string? RegistrationNumber { get; set; }

    public Guid VarietyId { get; set; }

    public string Name { get; set; } = null!;

    public decimal Size { get; set; }

    public int Age { get; set; }

    public string Gender { get; set; } = null!;

    public string? Bloodline { get; set; }

    public string ImgUrl { get; set; } = null!;

    public string VideoUrl { get; set; } = null!;

    public Guid CategoryId { get; set; }

    public Guid AccountId { get; set; }

    public decimal RegistrationFee { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }

    public DateTime? ApprovedAt { get; set; }
}
  