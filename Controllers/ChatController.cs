using Microsoft.AspNetCore.Mvc;
using QuotationSystem.Services;
using QuotationSystem.Models;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly ChatbotService _chatbotService;

    public ChatController(ChatbotService chatbotService)
    {
        _chatbotService = chatbotService;
    }

    [HttpPost("message")]
    public IActionResult PostMessage([FromBody] ChatMessageRequest request)
    {
        var response = _chatbotService.HandleMessage(request);
        return Ok(response);
    }
}
