using KSMS.Domain.Dtos.Responses.Registration;

namespace KSMS.Domain.Dtos.Responses.RegistrationRound;

public class CheckQrRegistrationRoundResponse
{
    public Guid Id { get; set; }
    public CheckQRRegistrationResoponse? Registration { get; set; }
}