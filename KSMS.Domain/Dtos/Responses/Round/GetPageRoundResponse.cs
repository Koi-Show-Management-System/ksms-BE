namespace KSMS.Domain.Dtos.Responses.Round;

public class GetPageRoundResponse
{
    public Guid Id { get; set; }
    public string? Name { get; set; }

    public int? RoundOrder { get; set; }

    public string? RoundType { get; set; }

    public string? Status { get; set; }
}