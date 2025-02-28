using System.ComponentModel.DataAnnotations;

namespace KSMS.Domain.Dtos.Requests.ErrorType;

public class CreateErrorTypeRequest
{
    [Required]
    public Guid CriteriaId { get; set; }
    [Required]
    public required string Name { get; set; }
}