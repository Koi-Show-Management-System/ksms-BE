using KSMS.Domain.Dtos;
using KSMS.Domain.Exceptions;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace KSMS.API.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong while processing {context.Request.Path}");
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
             var errorResponse = new ErrorResponse() { TimeStamp = DateTime.UtcNow, Error = ex.Message };

            errorResponse = ex switch
            {
                NotFoundException => errorResponse with { StatusCode = (int)HttpStatusCode.NotFound },
                BadRequestException => errorResponse with { StatusCode = (int)HttpStatusCode.BadRequest },
                UnauthorizedException => errorResponse with { StatusCode = (int)HttpStatusCode.Unauthorized },
                ForbiddenMethodException => errorResponse with { StatusCode= (int)HttpStatusCode.Forbidden },
                _ => errorResponse
            };

            var response = JsonConvert.SerializeObject(errorResponse);
            context.Response.StatusCode = errorResponse.StatusCode;
            return context.Response.WriteAsync(response);
        }
    }
}
