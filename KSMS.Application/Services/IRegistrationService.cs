using System.Security.Claims;
using KSMS.Domain.Dtos.Requests.Registration;
using KSMS.Domain.Dtos.Responses.CompetitionCategory;
using KSMS.Domain.Dtos.Responses.KoiShow;
using KSMS.Domain.Dtos.Responses.Registration;
using KSMS.Domain.Entities;
using KSMS.Domain.Enums;
using KSMS.Domain.Models;
using KSMS.Domain.Pagination;
using ShowStatus = KSMS.Domain.Enums.ShowStatus;

namespace KSMS.Application.Services;

public interface IRegistrationService
{
    Task<List<Guid>> GetRegistrationIdsByKoiShowAsync(Guid koiShowId);
    Task<object> CreateRegistration(CreateRegistrationRequest createRegistrationRequest);
    Task<List<GetPageCompetitionCategoryResponse>> FindSuitableCategoryAsync(Guid koiShowId, Guid varietyId, decimal size);
    Task UpdateRegistrationPaymentStatusForPayOs(Guid registrationPaymentId, RegistrationPaymentStatus status);
    Task UpdateStatusForRegistration(Guid registrationId, RegistrationStatus status, string? rejectedReason, RefundType? refundType);
    Task<CheckOutRegistrationResponse> CheckOut(Guid registrationId);
    
    Task<Paginate<GetPageRegistrationHistoryResponse>> GetPageRegistrationHistory(
        RegistrationStatus? registrationStatus, ShowStatus? showStatus, int page, int size);
    Task<GetShowMemberDetailResponse> GetMemberRegisterShowDetail(Guid showId);
    Task<Paginate<GetRegistrationResponse>> GetAllRegistrationForCurrentMember(RegistrationFilter filter, int page,
         int size);
}