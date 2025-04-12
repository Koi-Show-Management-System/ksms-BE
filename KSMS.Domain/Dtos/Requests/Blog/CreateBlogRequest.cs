using System.ComponentModel.DataAnnotations;

namespace KSMS.Domain.Dtos.Requests.Blog;

public class CreateBlogRequest
{
    [Required]
    public string Title { get; set; } = null!;
    [Required]
    public string Content { get; set; } = null!;
    [Required]
    public Guid BlogCategoryId { get; set; }
    public string? ImgUrl { get; set; }
}