using QuotationSystem.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace QuotationSystem.Services
{
    public class ChatbotService
    {
        private readonly DbHelper _db;
        private readonly ItemSearchService _itemSearchService;
        private readonly QuotationService _quotationService;
        private static readonly ConcurrentDictionary<string, ChatSessionState> Sessions = new();

        private static readonly List<string> StockGroups = new()
        {
            "Furniture", "Lighting", "Bedding Accessories", "Raw Materials", "Storage", "Others"
        };

        public ChatbotService(DbHelper db, ItemSearchService itemSearchService, QuotationService quotationService)
        {
            _db = db;
            _itemSearchService = itemSearchService;
            _quotationService = quotationService;
        }

        public ChatMessageResponse HandleMessage(ChatMessageRequest request)
        {
            var sessionId = request.SessionId ?? string.Empty;
            var message = request.Message?.Trim() ?? "";

            // Load or create session state
            var state = Sessions.GetOrAdd(sessionId, _ => new ChatSessionState());

            // Log message to DB
            LogMessage(sessionId, "client", message);

            // Step A: Ask search mode if missing
            if (string.IsNullOrEmpty(state.SearchMode))
            {
                state.Step = "AskSearchMode";
                return new ChatMessageResponse
                {
                    Reply = "Do you want to search by Category (StockGroup) or by Name (Description)?",
                    Step = state.Step,
                    Options = new List<string> { "StockGroup", "Description" },
                    Products = new List<ItemSearchResult>()
                };
            }

            // Step B: Ask for stock group
            if (state.SearchMode == "StockGroup" && string.IsNullOrEmpty(state.StockGroup))
            {
                state.Step = "AskStockGroup";
                return new ChatMessageResponse
                {
                    Reply = "Please choose a category:",
                    Step = state.Step,
                    Options = StockGroups,
                    Products = new List<ItemSearchResult>()
                };
            }

            // Step C: Ask for keyword
            if (state.SearchMode == "Description" && string.IsNullOrEmpty(state.Keyword))
            {
                state.Step = "AskKeyword";
                return new ChatMessageResponse
                {
                    Reply = "Please enter a keyword for item name:",
                    Step = state.Step,
                    Options = new List<string>(),
                    Products = new List<ItemSearchResult>()
                };
            }

            // Step D: Search items
            var searchReq = new ItemSearchRequest
            {
                SearchMode = state.SearchMode ?? string.Empty,
                StockGroup = state.StockGroup ?? string.Empty,
                Keyword = state.Keyword ?? string.Empty,
                Limit = 8
            };
            var products = _itemSearchService.SearchItems(searchReq);

            if (products.Count == 0)
            {
                state.Step = "NoProducts";
                return new ChatMessageResponse
                {
                    Reply = "No products found. Please broaden your keyword or switch search mode.",
                    Step = state.Step,
                    Options = new List<string> { "StockGroup", "Description" },
                    Products = new List<ItemSearchResult>()
                };
            }

            // Step E: Parse selection input
            var selection = ParseSelection(message, products);
            if (selection != null)
            {
                // Step F: Create quotation draft
                var draftReq = new QuotationDraftRequest
                {
                    SessionId = sessionId,
                    SearchMode = state.SearchMode ?? string.Empty,
                    StockGroup = state.StockGroup ?? string.Empty,
                    Keyword = state.Keyword ?? string.Empty,
                    SelectedItems = new List<SelectedItem> { selection }
                };
                var draftResp = _quotationService.CreateDraft(draftReq);

                state.Step = "DraftCreated";
                return new ChatMessageResponse
                {
                    Reply = $"Quotation draft created. Total: {draftResp.Total:F2}. Status: {draftResp.Status}",
                    Step = state.Step,
                    Options = new List<string>(),
                    Products = products
                };
            }

            // Show products
            state.Step = "ShowProducts";
            var numberedList = new List<string>();
            for (int i = 0; i < products.Count; i++)
            {
                var p = products[i];
                var priceStr = p.StockValue.HasValue ? $"{p.StockValue:F2}" : "not set";
                numberedList.Add($"{i + 1}. {p.Code} - {p.Description} ({p.StockGroup}) Price: {priceStr}");
            }

            return new ChatMessageResponse
            {
                Reply = "Top matches:\n" + string.Join("\n", numberedList) + "\nReply with 'choose N qty X' or 'code CODE qty X' to select.",
                Step = state.Step,
                Options = new List<string>(),
                Products = products
            };
        }

        private SelectedItem ParseSelection(string message, List<ItemSearchResult> products)
        {
            // choose 1 qty 2, choose 2, code A001 qty 2
            var m = Regex.Match(message, @"choose\s*(\d+)(?:\s*qty\s*(\d+))?", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                int idx = int.Parse(m.Groups[1].Value) - 1;
                int qty = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 1;
                if (idx >= 0 && idx < products.Count)
                {
                    return new SelectedItem { Code = products[idx].Code, Qty = qty };
                }
            }
            m = Regex.Match(message, @"code\s*(\w+)(?:\s*qty\s*(\d+))?", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                string code = m.Groups[1].Value;
                int qty = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 1;
                if (products.Exists(p => p.Code == code))
                {
                    return new SelectedItem { Code = code, Qty = qty };
                }
            }
            return default;
        }

        private void LogMessage(string sessionId, string sender, string text)
        {
            using var conn = _db.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
INSERT INTO CHAT_MESSAGE (SESSION_ID, SENDER, MESSAGE_TEXT, CREATED_AT, SENDER_TYPE)
VALUES (@sessionId, @sender, @text, CURRENT_TIMESTAMP, 'customer')";
            cmd.Parameters.AddWithValue("@sessionId", sessionId);
            cmd.Parameters.AddWithValue("@sender", sender);
            cmd.Parameters.AddWithValue("@text", text);
            cmd.ExecuteNonQuery();
        }
    }

    public class ChatSessionState
    {
        public string? SearchMode { get; set; }
        public string? StockGroup { get; set; }
        public string? Keyword { get; set; }
        public string? Step { get; set; }
    }
}
