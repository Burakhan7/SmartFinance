using Microsoft.EntityFrameworkCore;
using SmartFinance.Application.Common.Interfaces;
using SmartFinance.Domain.Entities;
using SmartFinance.Domain.Enums;
using SmartFinance.Infrastructure.Persistence;

namespace SmartFinance.Infrastructure.Offers;

public class OfferEngine : IOfferEngine
{
    private readonly AppDbContext _db;
    private readonly ISpendingAnalyzer _analyzer;

    public OfferEngine(AppDbContext db, ISpendingAnalyzer analyzer)
    {
        _db = db;
        _analyzer = analyzer;
    }

    public async Task<IReadOnlyList<Offer>> GenerateOffersAsync(
        Guid customerId, CancellationToken ct = default)
    {
        var analysis = await _analyzer.AnalyzeAsync(customerId, ct);
        var offers = new List<Offer>();

        // ═══ DAY-1 FIRSATLARI (ilk günden, geçmişe bakarak — bekleme yok) ═══

        // KURAL 1 [Insight]: Paran nereye gidiyor? (ayna etkisi)
        offers.Add(new Offer
        {
            CustomerId = customerId,
            Type = OfferType.Insight,
            Title = "Paran nereye gidiyor?",
            Description = $"Son {analysis.DataMonths} ayda toplam {analysis.TotalSpent:N0} TL harcadın. " +
                          $"En çok {CategoryLabel(analysis.TopCategory).ToLower()} kategorisinde " +
                          $"({analysis.TopCategoryAmount:N0} TL, %{analysis.TopCategoryPercent}). " +
                          $"Harcamalarını kategori kategori görebilirsin."
        });

        // KURAL 2 [Saving]: Abonelik röntgeni — YILLIK maliyetle vur
        if (analysis.Subscriptions.Count > 0)
        {
            var names = string.Join(", ", analysis.Subscriptions.Select(s => s.MerchantName));
            var monthly = analysis.Subscriptions.Sum(s => s.MonthlyAmount);
            offers.Add(new Offer
            {
                CustomerId = customerId,
                Type = OfferType.Saving,
                Title = "Aboneliklerin sana yılda ne kadara mal oluyor?",
                Description = $"{analysis.Subscriptions.Count} düzenli aboneliğin var ({names}). " +
                              $"Aylık {monthly:N0} TL, yani yılda {analysis.AnnualSubscriptionCost:N0} TL. " +
                              $"Kullanmadıklarını iptal ederek anında tasarruf edebilirsin."
            });
        }

        // KURAL 3 [Campaign]: En çok gittiğin yere özel kampanya
        if (analysis.TopCategoryAmount > 0)
        {
            offers.Add(new Offer
            {
                CustomerId = customerId,
                Type = OfferType.Campaign,
                Title = $"{CategoryLabel(analysis.TopCategory)} kampanyaları senin için",
                Description = $"En çok {CategoryLabel(analysis.TopCategory).ToLower()} " +
                              $"harcaması yapıyorsun. Bu kategoride sana özel indirimli " +
                              $"fırsatları kaçırma."
            });
        }

        // ═══ ZAMAN GEREKTİREN FIRSATLAR (yeterli geçmiş varsa) ═══

        // KURAL 4 [Trend]: Sadece en az 2 aylık veri varsa anlamlı
        if (analysis.DataMonths >= 2 && analysis.LastMonthSpent > 0)
        {
            if (analysis.ThisMonthSpent > analysis.LastMonthSpent * 1.25m)
            {
                var artis = Math.Round((analysis.ThisMonthSpent - analysis.LastMonthSpent)
                                       / analysis.LastMonthSpent * 100, 0);
                offers.Add(new Offer
                {
                    CustomerId = customerId,
                    Type = OfferType.Trend,
                    Title = "Harcamalarında artış var",
                    Description = $"Bu ay geçen aya göre harcaman %{artis} arttı " +
                                  $"({analysis.LastMonthSpent:N0} TL → {analysis.ThisMonthSpent:N0} TL)."
                });
            }
            else if (analysis.ThisMonthSpent < analysis.LastMonthSpent * 0.85m)
            {
                var azalis = Math.Round((analysis.LastMonthSpent - analysis.ThisMonthSpent)
                                        / analysis.LastMonthSpent * 100, 0);
                offers.Add(new Offer
                {
                    CustomerId = customerId,
                    Type = OfferType.Trend,
                    Title = "Tebrikler, harcamanı azalttın! 🎉",
                    Description = $"Bu ay geçen aya göre %{azalis} daha az harcadın " +
                                  $"({analysis.LastMonthSpent:N0} TL → {analysis.ThisMonthSpent:N0} TL). Böyle devam!"
                });
            }
        }
        // Üretilen fırsatları DB'ye yaz
        _db.Offers.AddRange(offers);
        await _db.SaveChangesAsync(ct);

        return offers;
    }

    private static string CategoryLabel(TransactionCategory c) => c switch
    {
        TransactionCategory.Groceries => "Market",
        TransactionCategory.Dining => "Yeme-İçme",
        TransactionCategory.Fuel => "Akaryakıt",
        TransactionCategory.Travel => "Seyahat",
        TransactionCategory.Entertainment => "Eğlence",
        TransactionCategory.Utilities => "Fatura",
        TransactionCategory.Shopping => "Alışveriş",
        TransactionCategory.Health => "Sağlık",
        _ => "Diğer"
    };
}