using KSMS.Application.Services;
using KSMS.Domain.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KSMS.API.Controllers;
[Route("api/v1/vote")]
public class VoteController : ControllerBase
{
    private readonly IVoteService _voteService;
    public VoteController(IVoteService voteService)
    {
        _voteService = voteService;
    }
    [HttpGet("get-registration-for-voting/{showId:guid}")]
    [Authorize(Roles = "Member")]
    public async Task<ActionResult<ApiResponse<object>>> GetFinalRegistrationForVoting(Guid showId)
    {
        var result = await _voteService.GetFinalRegistration(showId);
        return Ok(ApiResponse<object>.Success(result, "Lấy danh sách bình chọn thành công"));
    }
    
    [HttpGet("staff/get-registration-for-voting/{showId:guid}")]
    [Authorize(Roles = "Admin, Staff, Manager")]
    public async Task<ActionResult<ApiResponse<object>>> GetRegistrationForVoting(Guid showId)
    {
        var result = await _voteService.GetVotingRegistrationsForStaff(showId);
        return Ok(ApiResponse<object>.Success(result, "Lấy danh sách bình chọn thành công"));
    }
    [HttpGet("result/{showId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> GetVotingResult(Guid showId)
    {
        var result = await _voteService.GetVotingResults(showId);
        return Ok(ApiResponse<object>.Success(result, "Lấy kết quả bình chọn thành công"));
    }
    [HttpPost("create/{registrationId:guid}")]
    [Authorize(Roles = "Member")]
    public async Task<ActionResult<ApiResponse<object>>> CreateVote(Guid registrationId)
    {
        await _voteService.CreateVote(registrationId);
        return StatusCode(201, ApiResponse<object>.Created(null, "Bình chọn thành công"));
    }
    [HttpPost("enable-voting/{showId:guid}")]
    [Authorize(Roles = "Admin, Staff, Manager")]
    public async Task<ActionResult<ApiResponse<object>>> EnableVoting(Guid showId)
    {
        await _voteService.EnableVoting(showId);
        return Ok(ApiResponse<object>.Success(null, "Đã kích hoạt chức năng bình chọn"));
    }
    [HttpPost("disable-voting/{showId:guid}")]
    [Authorize(Roles = "Admin, Staff, Manager")]
    public async Task<ActionResult<ApiResponse<object>>> DisableVoting(Guid showId)
    {
        await _voteService.DisableVoting(showId);
        return Ok(ApiResponse<object>.Success(null, "Đã tắt chức năng bình chọn"));
    }
}