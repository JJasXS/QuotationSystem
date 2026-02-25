namespace QuotationSystem.Models
{
    public class ItemSearchRequest
    {
        public string? SearchMode { get; set; }
        public string? StockGroup { get; set; }
        public string? Keyword { get; set; }
        public int Limit { get; set; }
    }
}
