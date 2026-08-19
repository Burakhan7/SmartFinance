using Microsoft.AspNetCore.Mvc;
using SmartFinance.Application.Common.Interfaces;

namespace SmartFinance.WebApi.Controllers;

[ApiController]
[Route("api/sync")]
public class SyncController : ControllerBase
{
    private readonly ITransactionSyncService _sync;
    public SyncController(ITransactionSyncService sync) => _sync = sync;

    [HttpPost("{customerId:guid}")]
    public async Task<IActionResult> Sync(Guid customerId, CancellationToken ct)
    {
        try
        {
            var count = await _sync.SyncCustomerAsync(customerId, ct);
            return Ok(new { customerId, eklenenHareket = count });
        }
        catch (InvalidOperationException ex)
        {
            // Rıza yoksa 403 Forbidden dönmek doğru (yetki yok)
            return StatusCode(403, new { hata = ex.Message });
        }
    }
}