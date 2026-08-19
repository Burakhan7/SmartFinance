using Microsoft.AspNetCore.Mvc;
using SmartFinance.Application.Common.Interfaces;

namespace SmartFinance.WebApi.Controllers;

[ApiController]
[Route("api/customers/{customerId:guid}/consent")]
public class ConsentController : ControllerBase
{
    private readonly IConsentService _consent;
    public ConsentController(IConsentService consent) => _consent = consent;

    [HttpPost]
    public async Task<IActionResult> Grant(Guid customerId, CancellationToken ct)
    {
        var record = await _consent.GrantConsentAsync(customerId, 90, ct);
        return Ok(new { record.Id, record.GrantedAt, record.ExpiresAt, aktif = record.IsActive });
    }

    [HttpDelete]
    public async Task<IActionResult> Revoke(Guid customerId, CancellationToken ct)
    {
        await _consent.RevokeConsentAsync(customerId, ct);
        return Ok(new { mesaj = "Rıza geri çekildi, veri akışı durduruldu." });
    }

    [HttpGet]
    public async Task<IActionResult> Status(Guid customerId, CancellationToken ct)
    {
        var active = await _consent.HasActiveConsentAsync(customerId, ct);
        return Ok(new { customerId, aktifRiza = active });
    }
}