namespace KSMS.Domain.Dtos.Requests.Sponsor;

public class UpdateSponsorRequestV2
{
    public string Name { get; set; } = null!;
    public string? LogoUrl { get; set; }
    public decimal InvestMoney { get; set; }
}