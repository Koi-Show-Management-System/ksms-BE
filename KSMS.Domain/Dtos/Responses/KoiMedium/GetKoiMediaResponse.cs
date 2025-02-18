namespace KSMS.Domain.Dtos.Responses.KoiMedium;

public class GetKoiMediaResponse
{
    public Guid Id { get; set; }

    public string MediaUrl { get; set; } = null!;

    public string MediaType { get; set; } = null!;
}