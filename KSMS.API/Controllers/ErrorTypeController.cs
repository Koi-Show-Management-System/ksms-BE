using KSMS.Application.Services;
using KSMS.Domain.Dtos;
using KSMS.Domain.Dtos.Requests.ErrorType;
using Microsoft.AspNetCore.Mvc;

namespace KSMS.API.Controllers;
[Route("api/v1/error-type")]
[ApiController]
public class ErrorTypeController : ControllerBase
{
    private readonly IErrorTypeService _errorTypeService;

    public ErrorTypeController(IErrorTypeService errorTypeService)
    {
        _errorTypeService = errorTypeService;
    }
    [HttpPost("create")]
    public async Task<ActionResult<ApiResponse<object>>> CreateErrorType([FromBody] CreateErrorTypeRequest request)
    { 
        var errorType = await _errorTypeService.CreateErrorType(request);
        return StatusCode(201, ApiResponse<object>.Created(errorType, "Created Error Type successfully"));
    }
}