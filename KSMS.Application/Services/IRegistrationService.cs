using System.Security.Claims;
using KSMS.Domain.Dtos.Requests.Registration;
using KSMS.Domain.Dtos.Responses.Registration;
using KSMS.Domain.Enums;
using KSMS.Domain.Models;
using KSMS.Domain.Pagination;

namespace KSMS.Application.Services;

public interface IRegistrationService
{
    Task CreateRegistration(CreateRegistrationRequest createRegistrationRequest); 
    Task UpdateRegistrationPaymentStatusForPayOs(Guid registrationPaymentId, RegistrationPaymentStatus status);
     Task UpdateStatusForRegistration(Guid registrationId, RegistrationStatus status);
     Task<CheckOutRegistrationResponse> CheckOut(Guid registrationId);

     Task<Paginate<GetRegistrationResponse>> GetAllRegistrationForCurrentMember(RegistrationFilter filter, int page,
         int size);
}