namespace KSMS.Domain.Dtos.Responses.Sponsor;

public class SponsorGetKoiShowDetailResponse
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? LogoUrl { get; set; }

    public decimal InvestMoney { get; set; }
}