using System.ComponentModel.DataAnnotations;

namespace KSMS.Domain.Dtos.Requests.BlogCategory;

public class CreateBlogCategoryRequest
{
    [Required]
    public required string Name { get; set; }
    [Required]
    public required string Description { get; set; }
}