namespace QuotationSystem.Models
{
    public class QuotationDraftResponse
    {
        public long RequestId { get; set; }
        public string? Status { get; set; }
        public decimal Total { get; set; }
    }
}
