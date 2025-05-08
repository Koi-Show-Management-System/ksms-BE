namespace KSMS.Domain.Dtos.Responses.Account;

public class AccountGetCompetitonCategoryDetailResponse
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? Username { get; set; }
    public string? FullName { get; set; }
    public string? Role { get; set; }
    public string? Avatar { get; set; }
}