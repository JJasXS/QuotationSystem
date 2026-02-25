using FirebirdSql.Data.FirebirdClient;
using QuotationSystem.Models;
using System;
using System.Collections.Generic;

namespace QuotationSystem.Services
{
    public class QuotationService
    {
        private readonly DbHelper _db;
        private readonly ItemSearchService _itemSearchService;

        public QuotationService(DbHelper db, ItemSearchService itemSearchService)
        {
            _db = db;
            _itemSearchService = itemSearchService;
        }

        public QuotationDraftResponse CreateDraft(QuotationDraftRequest request)
        {
            using var conn = _db.GetConnection();
            using var tx = conn.BeginTransaction();

            // Insert QUOTATION_REQUEST
            var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"
INSERT INTO QUOTATION_REQUEST (SESSION_ID, SEARCH_MODE, STOCKGROUP, KEYWORDS_CSV, STATUS, CREATED_AT)
VALUES (@sessionId, @searchMode, @stockGroup, @keywordsCsv, 'PendingApproval', CURRENT_TIMESTAMP)
RETURNING REQUEST_ID";
            cmd.Parameters.AddWithValue("@sessionId", request.SessionId);
            cmd.Parameters.AddWithValue("@searchMode", request.SearchMode);
            cmd.Parameters.AddWithValue("@stockGroup", request.StockGroup ?? "");
            cmd.Parameters.AddWithValue("@keywordsCsv", request.SearchMode == "Description" ? request.Keyword ?? "" : "");

            long requestId = (long)cmd.ExecuteScalar();

            decimal total = 0;
            foreach (var item in request.SelectedItems)
            {
                // Get item snapshot
                var searchReq = new ItemSearchRequest
                {
                    SearchMode = "Description",
                    Keyword = item.Code,
                    Limit = 1
                };
                var found = _itemSearchService.SearchItems(searchReq);
                var snap = found.Count > 0 ? found[0] : null;

                decimal price = snap?.StockValue ?? 0;
                decimal qty = item.Qty > 0 ? item.Qty : 1;
                decimal lineTotal = price * qty;
                total += lineTotal;

                var cmdItem = conn.CreateCommand();
                cmdItem.Transaction = tx;
                cmdItem.CommandText = @"
INSERT INTO QUOTATION_REQUEST_ITEM
(REQUEST_ID, ITEM_CODE, DESCRIPTION_SNAPSHOT, UOM_SNAPSHOT, UNIT_PRICE_SNAPSHOT, QTY, LINE_TOTAL)
VALUES (@requestId, @code, @desc, @uom, @price, @qty, @lineTotal)";
                cmdItem.Parameters.AddWithValue("@requestId", requestId);
                cmdItem.Parameters.AddWithValue("@code", item.Code);
                cmdItem.Parameters.AddWithValue("@desc", snap?.Description ?? "");
                cmdItem.Parameters.AddWithValue("@uom", snap?.Uom ?? "");
                cmdItem.Parameters.AddWithValue("@price", price);
                cmdItem.Parameters.AddWithValue("@qty", qty);
                cmdItem.Parameters.AddWithValue("@lineTotal", lineTotal);
                cmdItem.ExecuteNonQuery();
            }

            tx.Commit();

            return new QuotationDraftResponse
            {
                RequestId = requestId,
                Status = "PendingApproval",
                Total = total
            };
        }
    }
}
