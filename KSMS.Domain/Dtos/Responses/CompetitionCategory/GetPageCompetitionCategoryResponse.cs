namespace KSMS.Domain.Dtos.Responses.CompetitionCategory;

public class GetPageCompetitionCategoryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;

    public decimal? SizeMin { get; set; }

    public decimal? SizeMax { get; set; }

    public string? Description { get; set; }

    public int? MaxEntries { get; set; }
    
    public bool? HasTank { get; set; }
    
    public decimal RegistrationFee { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }
    public List<string> Varieties { get; set; } = [];
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}