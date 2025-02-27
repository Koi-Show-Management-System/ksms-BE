using KSMS.Domain.Dtos.Responses.KoiShow;

namespace KSMS.Domain.Dtos.Responses.TicketType;

public class TicketTypeGetOrderDetailResponse
{
    public string Name { get; set; } = null!;
    
    public KoiShowGetOrderDetailResponse? KoiShow { get; set; }
}