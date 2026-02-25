using System.Collections.Generic;

namespace QuotationSystem.Models
{
    public class QuotationDraftRequest
    {
        public string? SessionId { get; set; }
        public string? SearchMode { get; set; }
        public string? StockGroup { get; set; }
        public string? Keyword { get; set; }
        public List<SelectedItem> SelectedItems { get; set; } = new();
    }

    public class SelectedItem
    {
        public string? Code { get; set; }
        public decimal Qty { get; set; }
    }
}
