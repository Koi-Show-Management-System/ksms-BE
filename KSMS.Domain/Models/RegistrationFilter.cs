using KSMS.Domain.Enums;

namespace KSMS.Domain.Models;

public class RegistrationFilter
{
    public List<Guid> ShowIds { get; set; } = [];
    public List<Guid> KoiProfileIds { get; set; } = [];
    public List<Guid> CategoryIds { get; set; } = [];
    public string? RegistrationNumber { get; set; }
    public RegistrationStatus? Status { get; set; }
}