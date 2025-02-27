using KSMS.Domain.Dtos.Responses.Account;

namespace KSMS.Domain.Dtos.Responses.Ticket;

public class GetTicketByOrderDetailResponse
{
    public Guid Id { get; set; }
    public string? QrcodeData { get; set; }

    public DateTime ExpiredDate { get; set; }

    public bool? IsCheckedIn { get; set; }

    public DateTime? CheckInTime { get; set; }

    public string? CheckInLocation { get; set; }

    public AccountGetTicketByOrderDetailResponse? CheckedInByNavigation { get; set; }

    public string? Status { get; set; }
}