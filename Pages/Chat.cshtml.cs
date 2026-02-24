using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace QuotationSystem.Pages
{
    public class ChatModel : PageModel
    {
        [BindProperty]
        public string? UserMessage { get; set; }
        public List<ChatMessage> Messages { get; set; } = new();

        public void OnGet()
        {
            // Optionally, add a welcome message
            Messages.Add(new ChatMessage { Text = "Hello! How can I help you today?", IsUser = false });
        }

        public void OnPost()
        {
            if (!string.IsNullOrWhiteSpace(UserMessage))
            {
                Messages.Add(new ChatMessage { Text = UserMessage, IsUser = true });
                // Simple bot reply (replace with real logic as needed)
                Messages.Add(new ChatMessage { Text = $"You said: {UserMessage}", IsUser = false });
            }
        }
    }

    public class ChatMessage
    {
        public string Text { get; set; } = string.Empty;
        public bool IsUser { get; set; }
    }
}
