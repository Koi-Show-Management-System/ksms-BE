using KSMS.Domain.Dtos.Requests.Authentication;
using KSMS.Domain.Dtos.Responses;

namespace KSMS.Application.Services;

public interface IAuthenticationService 
{
    Task Register(RegisterRequest registerRequest);
    Task<LoginResponse> Login(LoginRequest loginRequest);
}