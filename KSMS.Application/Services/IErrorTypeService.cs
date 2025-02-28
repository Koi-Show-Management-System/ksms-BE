using KSMS.Domain.Dtos.Requests.ErrorType;

namespace KSMS.Application.Services;

public interface IErrorTypeService
{
    Task<object> CreateErrorType(CreateErrorTypeRequest request);
}