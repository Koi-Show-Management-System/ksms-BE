using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.KoiProfile;
using KSMS.Domain.Dtos.Responses.KoiProfile;
using KSMS.Domain.Models;
using KSMS.Domain.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KSMS.API.Controllers
{
    [Route("api/koi-profle")]
    [ApiController]
    public class KoiProfileController : ControllerBase
    {
        private readonly IKoiProfileService _koiProfileService;

        public KoiProfileController(IKoiProfileService koiProfileService)
        {
            _koiProfileService = koiProfileService;
        }
        [HttpPost]
        [Authorize(Roles = "Member")]
        public async Task<ActionResult> CreateKoiProfile([FromForm] CreateKoiProfileRequest createKoiProfileRequest)
        {
            return CreatedAtAction(nameof(GetPagedKoiProfile),
                await _koiProfileService.CreateKoiProfile(HttpContext.User, createKoiProfileRequest));
        }
        [HttpGet]
        [Authorize(Roles = "Member")]
        public async Task<ActionResult<Paginate<GetAllKoiProfileResponse>>> GetPagedKoiProfile([FromQuery] KoiProfileFilter filter, [FromQuery]int page = 1, [FromQuery]int size = 10)
        {
            return await _koiProfileService.GetPagedKoiProfile(HttpContext.User, filter ,page, size);
        }
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Member")]
        public async Task<ActionResult<object>> UpdateKoiProfile(Guid id, [FromForm] UpdateKoiProfileRequest updateKoiProfileRequest)
        {
            return await _koiProfileService.UpdateKoiProfile(HttpContext.User, id, updateKoiProfileRequest);
        }
    }
}
