using System.Collections.Generic;

namespace QuotationSystem.Models
{
    public class ChatMessageResponse
    {
        public string? Reply { get; set; }
        public string? Step { get; set; }
        public List<string> Options { get; set; } = new();
        public List<ItemSearchResult> Products { get; set; } = new();
    }
}
