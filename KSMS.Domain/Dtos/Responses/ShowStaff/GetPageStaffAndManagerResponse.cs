using KSMS.Domain.Dtos.Responses.Account;

namespace KSMS.Domain.Dtos.Responses.ShowStaff;

public class GetPageStaffAndManagerResponse
{
    public Guid Id { get; set; }
    
    public AccountGetPageStaffOrManagerResponse? Account { get; set; }

    public AccountGetPageStaffOrManagerResponse? AssignedByNavigation { get; set; }
    
    public DateTime AssignedAt { get; set; }
    
}