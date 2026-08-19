using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartFinance.Application.Common.Interfaces;
using SmartFinance.Infrastructure.Persistence;

namespace SmartFinance.WebApi.Controllers;

[ApiController]
[Route("api/customers/{customerId:guid}")]
public class TransactionsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ISpendingAnalyzer _analyzer;

    public TransactionsController(AppDbContext db, ISpendingAnalyzer analyzer)
    {
        _db = db;
        _analyzer = analyzer;
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions(Guid customerId, CancellationToken ct)
    {
        var txs = await _db.Transactions
            .Where(t => t.Card.CustomerId == customerId)
            .OrderByDescending(t => t.OccurredAt)
            .ToListAsync(ct);
        return Ok(txs);
    }

    [HttpGet("analysis")]
    public async Task<IActionResult> GetAnalysis(Guid customerId, CancellationToken ct)
    {
        var analysis = await _analyzer.AnalyzeAsync(customerId, ct);
        return Ok(analysis);
    }
}