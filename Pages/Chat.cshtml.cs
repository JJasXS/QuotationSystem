using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using QuotationSystem.Services;
using QuotationSystem.Models;

namespace QuotationSystem.Pages
{
    public class ChatModel : PageModel
    {
        // Support AJAX GET for StockGroup/Description
        public IActionResult OnGetChooseMode(string SelectedMode)
        {
            this.SelectedMode = SelectedMode;
            if (SelectedMode == "StockGroup")
            {
                Step = "AskStockGroup";
                BotOptions = StockGroups;
            }
            else if (SelectedMode == "Description")
            {
                Step = "AskKeyword";
                BotOptions.Clear();
            }
            else
            {
                Step = "AskSearchMode";
                BotOptions = new List<string> { "StockGroup", "Description" };
            }
            return Partial("_ChatPartial", this);
        }

        public IActionResult OnGetChooseStockGroup(string SelectedStockGroup)
        {
            this.SelectedStockGroup = SelectedStockGroup;
            Step = "ShowProducts";
            Products = _itemSearchService.SearchItems(new ItemSearchRequest {
                SearchMode = "StockGroup",
                StockGroup = SelectedStockGroup,
                Limit = 8
            });
            BotOptions.Clear();
            return Partial("_ChatPartial", this);
        }

        [BindProperty]
        public string? UserMessage { get; set; }
        [BindProperty]
        public string? SelectedMode { get; set; }
        [BindProperty]
        public string? Keyword { get; set; }
        [BindProperty]
        public string? SelectedStockGroup { get; set; }
        public string Step { get; set; } = "Start";
        public List<string> BotOptions { get; set; } = new();
        public List<string> StockGroups { get; set; } = new() { "Furniture", "Lighting", "Bedding Accessories", "Raw Materials", "Storage", "Others" };
        public List<ItemSearchResult> Products { get; set; } = new();

        private readonly ItemSearchService _itemSearchService;

        public ChatModel(ItemSearchService itemSearchService)
        {
            _itemSearchService = itemSearchService;
        }

        public void OnGet()
        {
            Step = "AskSearchMode";
            BotOptions = new List<string> { "StockGroup", "Description" };
        }

        public IActionResult OnPostChooseMode()
        {
            if (SelectedMode == "StockGroup")
            {
                Step = "AskStockGroup";
                BotOptions = StockGroups;
            }
            else if (SelectedMode == "Description")
            {
                Step = "AskKeyword";
                BotOptions.Clear();
            }
            else
            {
                Step = "AskSearchMode";
                BotOptions = new List<string> { "StockGroup", "Description" };
            }
            return Page();
        }

        public IActionResult OnPostChooseStockGroup()
        {
            Step = "ShowProducts";
            Products = _itemSearchService.SearchItems(new ItemSearchRequest {
                SearchMode = "StockGroup",
                StockGroup = SelectedStockGroup,
                Limit = 8
            });
            BotOptions.Clear();
            return Page();
        }

        public IActionResult OnPostEnterKeyword()
        {
            Step = "ShowProducts";
            Products = _itemSearchService.SearchItems(new ItemSearchRequest {
                SearchMode = "Description",
                Keyword = Keyword,
                Limit = 8
            });
            BotOptions.Clear();
            return Page();
        }
    }

    public class ChatMessage
    {
        public string Text { get; set; } = string.Empty;
        public bool IsUser { get; set; }
    }

    public class ChatMessageResponse
    {
        public string? Reply { get; set; }
        public string? Step { get; set; }
        public List<string> Options { get; set; } = new();
        public List<ItemSearchResult> Products { get; set; } = new();
    }

    // Removed duplicate ItemSearchResult definition. Use QuotationSystem.Models.ItemSearchResult instead.
}