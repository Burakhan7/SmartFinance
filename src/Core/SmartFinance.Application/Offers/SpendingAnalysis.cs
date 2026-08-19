using SmartFinance.Domain.Enums;

namespace SmartFinance.Application.Offers;

// Bir müşterinin harcama davranışının özeti
public class SpendingAnalysis
{
    public Guid CustomerId { get; set; }
    public decimal TotalSpent { get; set; }
    public int TransactionCount { get; set; }

    // Kategori bazında toplam harcama (Fuel -> 1500 TL gibi)
    public Dictionary<TransactionCategory, decimal> SpentByCategory { get; set; } = new();

    // Tespit edilen abonelikler (merchant adı -> aylık tutar)
    public List<DetectedSubscription> Subscriptions { get; set; } = new();

    // En çok harcanan kategori (kolay erişim için)
    public TransactionCategory TopCategory { get; set; }
    public decimal TopCategoryAmount { get; set; }
    // Trend: bu ay vs geçen ay toplam harcama
    public decimal ThisMonthSpent { get; set; }
    public decimal LastMonthSpent { get; set; }
    public decimal TopCategoryPercent { get; set; } // en çok harcanan kategorinin toplam içindeki payı

    // Elimizdeki verinin kaç ayı kapsadığı (Day-1 kararları için)
    public int DataMonths { get; set; }
    public decimal AnnualSubscriptionCost { get; set; } // aboneliklerin yıllık maliyeti
}

public class DetectedSubscription
{
    public string MerchantName { get; set; } = default!;
    public decimal MonthlyAmount { get; set; }
    public int OccurrenceCount { get; set; } // kaç ay tekrarlamış


}