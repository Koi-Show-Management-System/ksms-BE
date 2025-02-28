namespace KSMS.Application.Services;

public interface IErrorTypeService
{
    Task<object> CreateErrorType(string name);
}