using KSMS.Domain.Dtos.Responses.Registration;
using KSMS.Domain.Dtos.Responses.RoundResult;

namespace KSMS.Domain.Dtos.Responses.RegistrationRound;

public class GetPageRegistrationRoundResponse
{
    public Guid Id { get; set; }
    public GetRegistrationResponse? Registration { get; set; }
    
    public DateTime? CheckInTime { get; set; }

    public DateTime? CheckOutTime { get; set; }

    public string? TankName { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }

    public ICollection<GetRoundResultResponse> RoundResults { get; set; } = [];
}