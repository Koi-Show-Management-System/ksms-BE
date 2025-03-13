namespace KSMS.Domain.Dtos.Responses.KoiShow;

public class GetKoiShowResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Status { get; set; } = null!;
}