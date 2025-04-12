using KSMS.Domain.Dtos.Responses.Account;
using KSMS.Domain.Dtos.Responses.BlogCategory;

namespace KSMS.Domain.Dtos.Responses.Blog;

public class GetPageBlogResponse
{
    public Guid Id { get; set; }
    public string? ImgUrl { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public GetBlogCategoryResponse? BlogCategory { get; set; }
    public AccountResponse? Account { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
}