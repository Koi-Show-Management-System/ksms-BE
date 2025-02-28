using KSMS.Domain.Dtos.Responses.KoiShow;

namespace KSMS.Domain.Dtos.Responses.KoiProfile;

public class CompetitionCategoryCheckinResponse
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = null!;
    
    public KoiShowCheckinResponse KoiShow { get; set; } = null!;
}