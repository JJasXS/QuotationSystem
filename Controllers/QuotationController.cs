using Microsoft.AspNetCore.Mvc;
using QuotationSystem.Services;
using QuotationSystem.Models;

[ApiController]
[Route("api/quotation")]
public class QuotationController : ControllerBase
{
    private readonly QuotationService _quotationService;

    public QuotationController(QuotationService quotationService)
    {
        _quotationService = quotationService;
    }

    [HttpPost("draft")]
    public IActionResult CreateDraft([FromBody] QuotationDraftRequest request)
    {
        var response = _quotationService.CreateDraft(request);
        return Ok(response);
    }
}
