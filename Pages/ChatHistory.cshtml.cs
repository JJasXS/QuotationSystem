using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace QuotationSystem.Pages
{
    public class ChatHistoryModel : PageModel
    {
        public List<string> Chats { get; set; } = new();
        [BindProperty]
        public string? NewChatTitle { get; set; }

        public void OnGet()
        {
            // Example: Load chat titles from a persistent store or session
            if (TempData.ContainsKey("Chats"))
                Chats = TempData["Chats"] as List<string> ?? new List<string>();
            else
                Chats = new List<string> { "Chat 1", "Chat 2", "Chat 3" };
        }

        public IActionResult OnPostAddChat()
        {
            if (TempData.ContainsKey("Chats"))
                Chats = TempData["Chats"] as List<string> ?? new List<string>();
            else
                Chats = new List<string>();

            if (!string.IsNullOrWhiteSpace(NewChatTitle))
                Chats.Add(NewChatTitle);

            TempData["Chats"] = Chats;
            return RedirectToPage("Chat");
        }
    }
}
