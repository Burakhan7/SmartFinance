using Microsoft.AspNetCore.Mvc;
using SmartFinance.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using SmartFinance.Infrastructure.Persistence;

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

    // Kayıtlı fırsatları getir. Yoksa otomatik üret (Day-1 deneyimi için).
    [HttpGet]
    public async Task<IActionResult> GetOffers(
        Guid customerId,
        [FromServices] AppDbContext db,
        CancellationToken ct)
    {
        var offers = await db.Offers
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

        // Hiç fırsat yoksa, üretmeyi dene (kullanıcı boş ekran görmesin)
        if (offers.Count == 0)
        {
            var generated = await _engine.GenerateOffersAsync(customerId, ct);
            return Ok(generated);
        }

        return Ok(offers);
    }

    // Bir fırsatı "görüldü" işaretle (kullanıcı okuyunca)
    [HttpPost("{offerId:guid}/seen")]
    public async Task<IActionResult> MarkSeen(
        Guid customerId, Guid offerId,
        [FromServices] AppDbContext db,
        CancellationToken ct)
    {
        var offer = await db.Offers
            .FirstOrDefaultAsync(o => o.Id == offerId && o.CustomerId == customerId, ct);

        if (offer is null) return NotFound();

        offer.IsSeen = true;
        await db.SaveChangesAsync(ct);
        return Ok(new { offer.Id, offer.IsSeen });
    }
}