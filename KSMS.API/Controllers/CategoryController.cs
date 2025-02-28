using KSMS.Application.Services;
using KSMS.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using KSMS.Domain.Dtos;

namespace KSMS.API.Controllers
{
    [Route("api/v1/category")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        //[HttpGet("show/registrations")]
        //public async Task<ActionResult<ApiResponse<object>>> GetAllRegistrationsByShow(
        //    [FromQuery] Guid showId,       
        //    [FromQuery] int page = 1,     
        //    [FromQuery] int size = 10)    
        //{
        //    var registrations = await _categoryService.GetPagedRegistrationsInShow(showId, page, size);
        //    return Ok(ApiResponse<object>.Success(registrations, "Get the list of registrations successfully"));
        //}

    }
}
