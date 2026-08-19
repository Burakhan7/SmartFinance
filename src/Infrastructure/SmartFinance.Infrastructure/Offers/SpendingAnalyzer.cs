using Microsoft.EntityFrameworkCore;
using SmartFinance.Application.Common.Interfaces;
using SmartFinance.Application.Offers;
using SmartFinance.Domain.Enums;
using SmartFinance.Infrastructure.Persistence;

namespace SmartFinance.Infrastructure.Offers;

public class SpendingAnalyzer : ISpendingAnalyzer
{
    private readonly AppDbContext _db;
    public SpendingAnalyzer(AppDbContext db) => _db = db;

    public async Task<SpendingAnalysis> AnalyzeAsync(Guid customerId, CancellationToken ct = default)
    {
        var txs = await _db.Transactions
            .Where(t => t.Card.CustomerId == customerId
                     && t.Direction == TransactionDirection.Debit)
            .ToListAsync(ct);

        var analysis = new SpendingAnalysis
        {
            CustomerId = customerId,
            TransactionCount = txs.Count,
            TotalSpent = txs.Sum(t => t.Amount)
        };

        if (txs.Count == 0) return analysis;

        // --- Kategori bazında toplam ---
        analysis.SpentByCategory = txs
            .GroupBy(t => t.Category)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

        // --- En çok harcanan kategori ---
        var top = analysis.SpentByCategory.OrderByDescending(kv => kv.Value).First();
        analysis.TopCategory = top.Key;
        analysis.TopCategoryAmount = top.Value;

        // --- En çok harcanan kategorinin payı (%) ---
        analysis.TopCategoryPercent = analysis.TotalSpent > 0
            ? Math.Round(analysis.TopCategoryAmount / analysis.TotalSpent * 100, 1)
            : 0;

        // --- Abonelik tespiti (sadece mantıklı kategoriler, en az 2 tekrar, tutarlı tutar) ---
        var subscriptionCategories = new HashSet<TransactionCategory>
        {
            TransactionCategory.Entertainment,
            TransactionCategory.Utilities
        };

        analysis.Subscriptions = txs
            .Where(t => subscriptionCategories.Contains(t.Category))
            .GroupBy(t => t.MerchantName)
            .Where(g => g.Count() >= 2)
            .Select(g => new DetectedSubscription
            {
                MerchantName = g.Key,
                MonthlyAmount = Math.Round(g.Average(t => t.Amount), 2),
                OccurrenceCount = g.Count()
            })
            .Where(s => IsSubscriptionLike(txs, s.MerchantName))
            .ToList();

        // --- Bu ay vs geçen ay ---
        var now = DateTime.UtcNow;
        var thisMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastMonthStart = thisMonthStart.AddMonths(-1);

        analysis.ThisMonthSpent = txs
            .Where(t => t.OccurredAt >= thisMonthStart)
            .Sum(t => t.Amount);

        analysis.LastMonthSpent = txs
            .Where(t => t.OccurredAt >= lastMonthStart && t.OccurredAt < thisMonthStart)
            .Sum(t => t.Amount);

        // --- Veri kaç ayı kapsıyor? ---
        var oldest = txs.Min(t => t.OccurredAt);
        var monthsSpan = (now.Year - oldest.Year) * 12 + now.Month - oldest.Month + 1;
        analysis.DataMonths = Math.Max(1, monthsSpan);

        // --- Aboneliklerin yıllık maliyeti ---
        analysis.AnnualSubscriptionCost = analysis.Subscriptions.Sum(s => s.MonthlyAmount) * 12;

        return analysis;
    }

    private static bool IsSubscriptionLike(
        IEnumerable<Domain.Entities.Transaction> txs, string merchant)
    {
        var amounts = txs.Where(t => t.MerchantName == merchant)
                         .Select(t => t.Amount).ToList();
        if (amounts.Count < 2) return false;

        var avg = amounts.Average();
        if (avg == 0) return false;

        return amounts.All(a => Math.Abs(a - avg) / avg <= 0.15m);
    }
}