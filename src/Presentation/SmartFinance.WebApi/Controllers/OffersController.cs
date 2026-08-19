using Microsoft.AspNetCore.Mvc;
using SmartFinance.Application.Common.Interfaces;

namespace SmartFinance.WebApi.Controllers;

[ApiController]
[Route("api/customers/{customerId:guid}/offers")]
public class OffersController : ControllerBase
{
    private readonly IOfferEngine _engine;
    public OffersController(IOfferEngine engine) => _engine = engine;

    [HttpPost("generate")]
    public async Task<IActionResult> Generate(Guid customerId, CancellationToken ct)
    {
        var offers = await _engine.GenerateOffersAsync(customerId, ct);
        return Ok(offers);
    }
}