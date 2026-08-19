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

// Analizi gör (fırsat üretmeden, sadece özet)
app.MapGet("/customers/{customerId:guid}/analysis", async (
    Guid customerId, ISpendingAnalyzer analyzer, CancellationToken ct) =>
{
    var analysis = await analyzer.AnalyzeAsync(customerId, ct);
    return Results.Ok(analysis);
});

// Fırsat üret + DB'ye yaz
app.MapPost("/customers/{customerId:guid}/offers/generate", async (
    Guid customerId, IOfferEngine engine, CancellationToken ct) =>
{
    var offers = await engine.GenerateOffersAsync(customerId, ct);
    return Results.Ok(offers);
});

// Rıza ver (kart bağlama anı)
app.MapPost("/customers/{customerId:guid}/consent", async (
    Guid customerId, IConsentService consent, CancellationToken ct) =>
{
    var record = await consent.GrantConsentAsync(customerId, 90, ct);
    return Results.Ok(new { record.Id, record.GrantedAt, record.ExpiresAt, aktif = record.IsActive });
});

// Rıza geri çek
app.MapDelete("/customers/{customerId:guid}/consent", async (
    Guid customerId, IConsentService consent, CancellationToken ct) =>
{
    await consent.RevokeConsentAsync(customerId, ct);
    return Results.Ok(new { mesaj = "Rıza geri çekildi, veri akışı durduruldu." });
});

// Rıza durumu
app.MapGet("/customers/{customerId:guid}/consent", async (
    Guid customerId, IConsentService consent, CancellationToken ct) =>
{
    var active = await consent.HasActiveConsentAsync(customerId, ct);
    return Results.Ok(new { customerId, aktifRiza = active });
});

app.Run();