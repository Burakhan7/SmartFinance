using SmartFinance.Application.Common.Interfaces;
using SmartFinance.Infrastructure;
using Microsoft.EntityFrameworkCore;
using SmartFinance.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Test endpoint: mock hareketleri gözümüzle görelim
app.MapGet("/transactions/{customerId:guid}", async (
    Guid customerId, ITransactionProvider provider, CancellationToken ct) =>
{
    var from = DateTime.UtcNow.AddDays(-30);
    var to = DateTime.UtcNow;
    var transactions = await provider.GetTransactionsAsync(customerId, from, to, ct);
    return Results.Ok(transactions);
});

// Sağlayıcıdan çekip DB'ye yaz (biriktirme)
app.MapPost("/sync/{customerId:guid}", async (
    Guid customerId, ITransactionSyncService sync, CancellationToken ct) =>
{
    var count = await sync.SyncCustomerAsync(customerId, ct);
    return Results.Ok(new { customerId, eklenenHareket = count });
});

// DB'de KAYITLI olanları göster (provider'dan değil, depodan)
app.MapGet("/customers/{customerId:guid}/transactions", async (
    Guid customerId, AppDbContext db, CancellationToken ct) =>
{
    var txs = await db.Transactions
        .Where(t => t.Card.CustomerId == customerId)
        .OrderByDescending(t => t.OccurredAt)
        .ToListAsync(ct);
    return Results.Ok(txs);
});

app.Run();