using KSMS.Application.Services;
using KSMS.Domain.Dtos;
using KSMS.Domain.Dtos.Responses.Livestream;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KSMS.API.Controllers;
[Route("api/v1/livestream")]
[ApiController]
public class LivestreamController : ControllerBase
{
    private readonly ILivestreamService _livestreamService;

    public LivestreamController(ILivestreamService livestreamService)
    {
        _livestreamService = livestreamService;
    }
    [HttpPost("create/{koiShowId:guid}")]
    [Authorize(Roles = "Staff, Admin, Manager")]
    public async Task<ActionResult<ApiResponse<object>>> CreateLivestream(Guid koiShowId)
    {
        var response = await _livestreamService.CreateLivestream(koiShowId);
        return StatusCode(201, ApiResponse<object>.Created(response, "Tạo livestream thành công"));
    }
    [HttpPost("start/{id:guid}")]
    [Authorize(Roles = "Staff, Admin, Manager")]
    public async Task<ActionResult<ApiResponse<object>>> StartLiveStream(Guid id)
    {
        await _livestreamService.StartLivestream(id);
        return Ok(ApiResponse<object>.Success(null, "Bắt đầu livestream thành công"));
    }
    [HttpPost("end/{id:guid}")]
    [Authorize(Roles = "Staff, Admin, Manager")]
    public async Task<ActionResult<ApiResponse<object>>> EndLivestream(Guid id)
    {
        await _livestreamService.EndLivestream(id);
        return Ok(ApiResponse<object>.Success(null, "Kết thúc livestream thành công"));
    }
    [HttpGet("get-all/{koiShowId:guid}")]
    public async Task<ActionResult<ApiResponse<List<GetLiveStreamResponse>>>> GetLivestreams(Guid koiShowId)
    {
        var livestreams = await _livestreamService.GetLivestreams(koiShowId);
        return Ok(ApiResponse<List<GetLiveStreamResponse>>.Success(livestreams, "Lấy danh sách livestream thành công"));
    }
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<GetLiveStreamResponse>>> GetLivestreamById(Guid id)
    {
        var livestream = await _livestreamService.GetLivestreamById(id);
        return Ok(ApiResponse<GetLiveStreamResponse>.Success(livestream, "Lấy livestream thành công"));
    }
    [HttpGet("token/{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> GetTokenForLivestream(Guid id)
    {
        var token = await _livestreamService.GetLiveStreamHostToken(id);
        return Ok(ApiResponse<object>.Success(token, "Lấy token livestream thành công"));
    }
    [HttpGet("viewer-token/{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> GetViewerTokenForLivestream(Guid id)
    {
        var token = await _livestreamService.GetLiveStreamViewToken(id);
        return Ok(ApiResponse<object>.Success(token, "Lấy token livestream thành công"));
    }
}