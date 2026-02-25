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
        try
        {
            var response = _chatbotService.HandleMessage(request);
            if (response == null)
                return BadRequest(new { reply = "Bot error: No response." });
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { reply = "Bot error: " + ex.Message });
        }
    }
}
