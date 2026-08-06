using SmartFinance.Application.Common.Interfaces;
using SmartFinance.Domain.Entities;
using SmartFinance.Domain.Enums;

namespace SmartFinance.Infrastructure.Providers;

public class MockTransactionProvider : ITransactionProvider
{
    private static readonly (string Merchant, TransactionCategory Category)[] _merchants =
    {
        ("Migros",        TransactionCategory.Groceries),
        ("BİM",           TransactionCategory.Groceries),
        ("Starbucks",     TransactionCategory.Dining),
        ("Domino's",      TransactionCategory.Dining),
        ("Shell",         TransactionCategory.Fuel),
        ("Opet",          TransactionCategory.Fuel),
        ("THY",           TransactionCategory.Travel),
        ("Netflix",       TransactionCategory.Entertainment),
        ("Spotify",       TransactionCategory.Entertainment),
        ("Trendyol",      TransactionCategory.Shopping),
        ("Enerjisa",      TransactionCategory.Utilities),
        ("Eczane",        TransactionCategory.Health),
    };

    public Task<IReadOnlyList<Transaction>> GetTransactionsAsync(
        Guid customerId, DateTime from, DateTime to, CancellationToken ct = default)
    {
        var rng = new Random(customerId.GetHashCode()); // aynı müşteri → aynı veri (deterministik)
        var transactions = new List<Transaction>();

        var totalDays = Math.Max(1, (to - from).Days);
        var count = rng.Next(20, 60); // müşteri başına 20-60 hareket

        for (int i = 0; i < count; i++)
        {
            var (merchant, category) = _merchants[rng.Next(_merchants.Length)];

            transactions.Add(new Transaction
            {
                Id = Guid.NewGuid(),
                CardId = Guid.Empty, // ileride gerçek karta bağlanacak
                Amount = Math.Round((decimal)(rng.NextDouble() * 900 + 20), 2), // 20-920 TL
                Currency = "TRY",
                MerchantName = merchant,
                Category = category,
                Direction = TransactionDirection.Debit,
                OccurredAt = from.AddDays(rng.Next(totalDays))
                                  .AddHours(rng.Next(24))
                                  .AddMinutes(rng.Next(60)),
            });
        }

        var ordered = transactions
            .OrderByDescending(t => t.OccurredAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<Transaction>>(ordered);
    }
}