using System.Security.Claims;
using KSMS.Domain.Dtos.Requests.Registration;
using KSMS.Domain.Dtos.Responses.Registration;
using KSMS.Domain.Entities;
using KSMS.Domain.Enums;
using KSMS.Domain.Models;
using KSMS.Domain.Pagination;

namespace KSMS.Application.Services;

public interface IRegistrationService
{
    Task<List<Guid>> GetRegistrationIdsByKoiShowAsync(Guid koiShowId);
    Task<CheckQRRegistrationResoponse> GetRegistrationByIdAndRoundAsync(Guid registrationId, Guid roundId);
    Task<object> CreateRegistration(CreateRegistrationRequest createRegistrationRequest); 
    Task UpdateRegistrationPaymentStatusForPayOs(Guid registrationPaymentId, RegistrationPaymentStatus status);
    Task UpdateStatusForRegistration(Guid registrationId, RegistrationStatus status);
    Task<CheckOutRegistrationResponse> CheckOut(Guid registrationId);
 //   Task AssignAllFishToTankAndRound(Guid showId);
    Task AssignMultipleFishesToTankAndRound(Guid roundId, List<Guid> registrationIds);


   // Task UpdateRegistrationStatus(Guid registrationId, string status);
    Task<Paginate<GetRegistrationResponse>> GetAllRegistrationForCurrentMember(RegistrationFilter filter, int page,
         int size);
}