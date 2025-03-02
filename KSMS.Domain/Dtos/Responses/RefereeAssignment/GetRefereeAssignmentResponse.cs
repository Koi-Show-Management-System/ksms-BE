using KSMS.Domain.Dtos.Responses.Account;

namespace KSMS.Domain.Dtos.Responses.RefereeAssignment;

public class GetRefereeAssignmentResponse
{
    public Guid Id { get; set; }
   
    public string RoundType { get; set; } = null!;

    public DateTime AssignedAt { get; set; }
    public AccountGetCompetitonCategoryDetailResponse? RefereeAccount { get; set; }
    public AccountGetCompetitonCategoryDetailResponse? AssignedByNavigation { get; set; }
}