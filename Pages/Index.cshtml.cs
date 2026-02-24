using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QuotationSystem.Pages;

public class IndexModel : PageModel
{
    private readonly DbHelper _dbHelper;
    public string? DbResult { get; set; }
    public List<string>? TableNames { get; set; }

    public IndexModel(DbHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    public void OnGet()
    {
    }

    public void OnPost()
    {
        try
        {
            var sql = "SELECT RDB$RELATION_NAME AS TABLE_NAME FROM RDB$RELATIONS WHERE RDB$VIEW_BLR IS NULL AND (RDB$SYSTEM_FLAG IS NULL OR RDB$SYSTEM_FLAG = 0) ORDER BY RDB$RELATION_NAME";
            var rows = _dbHelper.ExecuteSelect(sql);
            TableNames = new List<string>();
            foreach (var row in rows)
            {
                var name = row["TABLE_NAME"]?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(name))
                    TableNames.Add(name);
            }
            DbResult = $"Found {TableNames.Count} tables.";
        }
        catch (Exception ex)
        {
            DbResult = $"Error: {ex.Message}";
        }
    }
}
