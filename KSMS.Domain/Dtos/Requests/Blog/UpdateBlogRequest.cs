using System.ComponentModel.DataAnnotations;

namespace KSMS.Domain.Dtos.Requests.Blog;

public class UpdateBlogRequest
{
    [Required]
    public required string Title { get; set; } 
    [Required]
    public required string Content { get; set; } 
    public Guid BlogCategoryId { get; set; }
    public string? ImgUrl { get; set; }
}