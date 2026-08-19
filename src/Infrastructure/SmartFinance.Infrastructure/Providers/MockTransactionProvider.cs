using SmartFinance.Application.Common.Interfaces;
using SmartFinance.Domain.Entities;
using SmartFinance.Domain.Enums;

namespace SmartFinance.Infrastructure.Providers;

public class MockTransactionProvider : ITransactionProvider
{
    // Rastgele (değişken tutarlı) harcamalar
    private static readonly (string Merchant, TransactionCategory Category)[] _merchants =
    {
        ("Migros",    TransactionCategory.Groceries),
        ("BİM",       TransactionCategory.Groceries),
        ("Starbucks", TransactionCategory.Dining),
        ("Domino's",  TransactionCategory.Dining),
        ("Shell",     TransactionCategory.Fuel),
        ("Opet",      TransactionCategory.Fuel),
        ("THY",       TransactionCategory.Travel),
        ("Trendyol",  TransactionCategory.Shopping),
        ("Eczane",    TransactionCategory.Health),
    };

    // Sabit tutarlı abonelikler: her ay aynı gün, aynı fiyat
    private static readonly (string Merchant, TransactionCategory Category, decimal Amount, int Day)[] _subscriptions =
    {
        ("Netflix",   TransactionCategory.Entertainment, 199.99m, 5),
        ("Spotify",   TransactionCategory.Entertainment,  79.99m, 12),
        ("Enerjisa",  TransactionCategory.Utilities,     450.00m, 20), // düzenli fatura
    };

    public Task<IReadOnlyList<Transaction>> GetTransactionsAsync(
        Guid customerId, DateTime from, DateTime to, CancellationToken ct = default)
    {
        // GUID'in byte'larından deterministik, kullanıcıya özel seed üret
        var seed = BitConverter.ToInt32(customerId.ToByteArray(), 0);
        var rng = new Random(seed);
        var transactions = new List<Transaction>();

        // Kaç ay geriye gidiyoruz (trend için son 3 ay)
        var monthsBack = Math.Max(1, ((to.Year - from.Year) * 12) + to.Month - from.Month);

        // 1) Her ay için abonelikleri ekle (sabit tutar, sabit gün)
        for (int m = 0; m <= monthsBack; m++)
        {
            var monthDate = to.AddMonths(-m);
            foreach (var sub in _subscriptions)
            {
                var day = Math.Min(sub.Day, DateTime.DaysInMonth(monthDate.Year, monthDate.Month));
                var date = new DateTime(monthDate.Year, monthDate.Month, day, 10, 0, 0, DateTimeKind.Utc);
                if (date < from || date > to) continue;

                transactions.Add(new Transaction
                {
                    CardId = Guid.Empty,
                    Amount = sub.Amount,
                    Currency = "TRY",
                    MerchantName = sub.Merchant,
                    Category = sub.Category,
                    Direction = TransactionDirection.Debit,
                    OccurredAt = date,
                });
            }
        }

        // 2) Rastgele günlük harcamalar (değişken tutar)
        var totalDays = Math.Max(1, (to - from).Days);
        var count = rng.Next(30, 60);

        for (int i = 0; i < count; i++)
        {
            var (merchant, category) = _merchants[rng.Next(_merchants.Length)];
            transactions.Add(new Transaction
            {
                CardId = Guid.Empty,
                Amount = Math.Round((decimal)(rng.NextDouble() * 900 + 20), 2),
                Currency = "TRY",
                MerchantName = merchant,
                Category = category,
                Direction = TransactionDirection.Debit,
                OccurredAt = from.AddDays(rng.Next(totalDays))
                                 .AddHours(rng.Next(24)).AddMinutes(rng.Next(60)),
            });
        }

        var ordered = transactions.OrderByDescending(t => t.OccurredAt).ToList();
        return Task.FromResult<IReadOnlyList<Transaction>>(ordered);
    }
}