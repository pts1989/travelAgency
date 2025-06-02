using IntelliTrip.Planner.UserInteraction.Models;
using IntelliTrip.Planner.UserInteraction.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntelliTrip.Planner.UserInteraction.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpPost]
    // [Obsolete("Direct agent response is now handled via SignalR/ChatHub. Use the chat UI for real-time updates.")]
    public IActionResult Post([FromBody] ChatMessageRequest request)
    {
        return StatusCode(410, "Deze endpoint wordt niet meer gebruikt. Gebruik de chat via SignalR.");
    }

    [HttpPost("stream")]
    public async Task Stream([FromBody] ChatMessageRequest request)
    {
        Response.ContentType = "text/plain";
        await foreach (var token in _chatService.StreamAgentResponseAsync(request, HttpContext.RequestAborted))
        {
            // Write each token as soon as it's available, but do NOT add extra newlines
            var buffer = System.Text.Encoding.UTF8.GetBytes(token);
            await Response.Body.WriteAsync(buffer, 0, buffer.Length, HttpContext.RequestAborted);
            await Response.Body.FlushAsync(HttpContext.RequestAborted);
        }
    }
}
