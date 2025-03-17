using KSMS.Domain.Dtos.Responses.KoiMedium;
using KSMS.Domain.Dtos.Responses.Variety;

namespace KSMS.Domain.Dtos.Responses.KoiProfile;

public class GetKoiDetailResponse
{
    public Guid Id { get; set; }
    public string? Name { get; set; }

    public decimal? Size { get; set; }

    public int? Age { get; set; }

    public string? Gender { get; set; }

    public string? Bloodline { get; set; }

    public string? Status { get; set; }
    
    public DateTime? CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }

    public ICollection<GetKoiMediaResponse> KoiMedia { get; set; } = new List<GetKoiMediaResponse>();
    
    public VarietyResponse? Variety { get; set; }

    public List<KoiAchievementResponse> Achievements { get; set; } = [];
    public List<KoiCompetitionHistoryResponse> CompetitionHistory { get; set; } = [];


}

public class KoiAchievementResponse
{
   public string? ShowName {get;set;}
   public string? Location { get; set; }
   public string? CategoryName { get; set; }
   public string? AwardType { get; set; }
   public decimal? PrizeValue { get; set; }
   public string? AwardName { get; set; }
   public DateTime? CompetitionDate { get; set; }
}

public class KoiCompetitionHistoryResponse
{
    public string? Year { get; set; }
    public string? ShowName { get; set; }
    public string? ShowStatus { get; set; }
    public string? Location { get; set; }
    public string? Result { get; set; }
}    