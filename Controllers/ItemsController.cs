using Microsoft.AspNetCore.Mvc;
using QuotationSystem.Services;
using QuotationSystem.Models;

[ApiController]
[Route("api/items")]
public class ItemsController : ControllerBase
{
    private readonly ItemSearchService _itemSearchService;

    public ItemsController(ItemSearchService itemSearchService)
    {
        _itemSearchService = itemSearchService;
    }

    [HttpPost("search")]
    public IActionResult SearchItems([FromBody] ItemSearchRequest request)
    {
        var items = _itemSearchService.SearchItems(request);
        return Ok(items);
    }
}
