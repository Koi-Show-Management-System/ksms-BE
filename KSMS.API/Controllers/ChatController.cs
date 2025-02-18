using Microsoft.AspNetCore.Mvc;
using KSMS.Application.Services;
using KSMS.Domain.Dtos;
using KSMS.Domain.Dtos.Requests;

namespace KSMS.API.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<string>>> GetChatResponse([FromBody] ChatRequest request)
        {
            var response = await _chatService.GetChatResponse(request.Question);
            return Ok(ApiResponse<string>.Success(response, "Chat response generated successfully"));
        }
    }

    
} 