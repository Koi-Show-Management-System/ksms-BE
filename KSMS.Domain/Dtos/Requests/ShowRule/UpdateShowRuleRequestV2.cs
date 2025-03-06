using System.ComponentModel.DataAnnotations;

namespace KSMS.Domain.Dtos.Requests.ShowRule;

public class UpdateShowRuleRequestV2
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title must not exceed 200 characters.")]
    public string Title { get; set; } = null!;

    [Required(ErrorMessage = "Content is required.")]
    public string Content { get; set; } = null!;
}