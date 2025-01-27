using System.Security.Claims;
using KSMS.Domain.Dtos.Requests.Registration;
using KSMS.Domain.Dtos.Responses.Registration;
using KSMS.Domain.Enums;

namespace KSMS.Application.Services;

public interface IRegistrationService
{
    Task<RegistrationResponse> CreateRegistrationWithPayOs(ClaimsPrincipal claims, CreateRegistrationRequest createRegistrationRequest);
    Task UpdateRegistrationPaymentStatusForPayOs(Guid registrationPaymentId, RegistrationPaymentStatus status);
}