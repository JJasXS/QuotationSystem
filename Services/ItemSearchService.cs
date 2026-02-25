using FirebirdSql.Data.FirebirdClient;
using QuotationSystem.Models;
using System.Collections.Generic;

namespace QuotationSystem.Services
{
    public class ItemSearchService
    {
        private readonly DbHelper _db;

        public ItemSearchService(DbHelper db)
        {
            _db = db;
        }

        public List<ItemSearchResult> SearchItems(ItemSearchRequest request)
        {
            var results = new List<ItemSearchResult>();
            using var conn = _db.GetConnection();
            using var cmd = conn.CreateCommand();

            string sql = @"
SELECT i.CODE, i.DESCRIPTION, i.STOCKGROUP, p.UOM, p.STOCKVALUE
FROM ST_ITEM i
LEFT JOIN (
    SELECT p.CODE, p.UOM, p.STOCKVALUE, p.DTLKEY
    FROM ST_ITEM_PRICE p
    WHERE (p.DATEFROM IS NULL OR p.DATEFROM <= CURRENT_DATE)
      AND (p.DATETO IS NULL OR p.DATETO >= CURRENT_DATE)
      AND p.DTLKEY = (
        SELECT FIRST 1 p2.DTLKEY
        FROM ST_ITEM_PRICE p2
        WHERE p2.CODE = p.CODE
          AND (p2.DATEFROM IS NULL OR p2.DATEFROM <= CURRENT_DATE)
          AND (p2.DATETO IS NULL OR p2.DATETO >= CURRENT_DATE)
        ORDER BY p2.DATEFROM DESC NULLS LAST, p2.SEQ DESC, p2.DTLKEY DESC
      )
) p ON p.CODE = i.CODE
WHERE i.ISACTIVE = 1
";

            if (request.SearchMode == "StockGroup" && !string.IsNullOrEmpty(request.StockGroup))
            {
                sql += " AND i.STOCKGROUP = @stockGroup";
                cmd.Parameters.AddWithValue("@stockGroup", request.StockGroup);
            }
            else if (request.SearchMode == "Description" && !string.IsNullOrEmpty(request.Keyword))
            {
                sql += " AND i.DESCRIPTION LIKE @keyword";
                cmd.Parameters.AddWithValue("@keyword", "%" + request.Keyword + "%");
            }

            sql += " ROWS " + (request.Limit > 0 ? request.Limit : 8);

            cmd.CommandText = sql;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                results.Add(new ItemSearchResult
                {
                    Code = reader.IsDBNull(0) ? null : reader.GetString(0),
                    Description = reader.IsDBNull(1) ? null : reader.GetString(1),
                    StockGroup = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Uom = reader.IsDBNull(3) ? null : reader.GetString(3),
                    StockValue = reader.IsDBNull(4) ? null : reader.GetDecimal(4)
                });
            }
            return results;
        }
    }
}
