namespace KSMS.Domain.Dtos.Responses.Livestream;

public class TokenResponse
{
    public string? Token { get; set; }
}
public class LivestreamTokenResponse
{
    public Guid Id { get; set; }
    public string? CallId { get; set; }
    public string? Token { get; set; }
}