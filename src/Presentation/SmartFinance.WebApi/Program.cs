using SmartFinance.Application.Common.Interfaces;
using SmartFinance.Infrastructure;

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

app.Run();