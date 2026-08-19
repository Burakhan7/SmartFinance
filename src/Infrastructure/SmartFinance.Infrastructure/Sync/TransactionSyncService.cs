using Microsoft.EntityFrameworkCore;
using SmartFinance.Application.Common.Interfaces;
using SmartFinance.Domain.Entities;
using SmartFinance.Domain.Enums;
using SmartFinance.Infrastructure.Persistence;

namespace SmartFinance.Infrastructure.Sync;

public class TransactionSyncService : ITransactionSyncService
{
    private readonly AppDbContext _db;
    private readonly ITransactionProvider _provider;
    private readonly IConsentService _consent;

    public TransactionSyncService(
        AppDbContext db, ITransactionProvider provider, IConsentService consent)
    {
        _db = db;
        _provider = provider;
        _consent = consent;
    }

    public async Task<int> SyncCustomerAsync(Guid customerId, CancellationToken ct = default)
    {

        // RIZA KONTROLÜ: aktif rıza yoksa veri çekme (yasal zorunluluk)
        if (!await _consent.HasActiveConsentAsync(customerId, ct))
        {
            throw new InvalidOperationException(
                "Bu müşteri için aktif rıza bulunmuyor. Önce rıza alınmalı.");
        }
        var customer = await _db.Customers
            .Include(c => c.Cards)
            .FirstOrDefaultAsync(c => c.Id == customerId, ct);

        // Rıza akışı müşteriyi zaten oluşturdu; buraya rızalı müşteri gelir.
        // Yine de güvenlik için: müşteri yoksa sync yapılmaz.
        if (customer is null)
        {
            throw new InvalidOperationException(
                "Müşteri bulunamadı. Sync öncesi rıza akışı tamamlanmalı.");
        }

        // 2. Kartı yoksa oluştur (hareketler bir karta bağlanmak zorunda)
        var card = customer.Cards.FirstOrDefault();
        if (card is null)
        {
            card = new Card
            {
                CustomerId = customer.Id,
                MaskedNumber = "**** **** **** 1234",
                Type = CardType.Credit
            };
            _db.Cards.Add(card);
            await _db.SaveChangesAsync(ct); // kartı ÖNCE kaydet ki Id DB'de hazır olsun
        }

        // 3. Sağlayıcıdan çek. Pencereyi UtcNow.Date'e sabitliyorum ki
        //    aynı gün tekrar sync'lersen aynı veri gelsin (mükerrer testi için).
        var to = DateTime.UtcNow.Date;
        var from = to.AddDays(-90);
        var incoming = await _provider.GetTransactionsAsync(customerId, from, to, ct);

        // 4. Zaten kayıtlı olanları ele. Gerçek entegrasyonda bankanın
        //    işlem referans no'su ile yapılır; mock'ta imza (tarih+tutar+merchant) ile.
        var existing = await _db.Transactions
            .Where(t => t.CardId == card.Id)
            .Select(t => new { t.OccurredAt, t.Amount, t.MerchantName })
            .ToListAsync(ct);

        var existingKeys = existing
    .Select(t => $"{t.OccurredAt.Ticks}|{t.Amount}|{t.MerchantName}")
    .ToHashSet();

        var newOnes = new List<Transaction>();
        foreach (var tx in incoming)
        {
            var key = $"{tx.OccurredAt.Ticks}|{tx.Amount}|{tx.MerchantName}";
            if (existingKeys.Contains(key)) continue; // zaten var, atla

            tx.CardId = card.Id; // mock'ta boştu, gerçek karta bağla
            newOnes.Add(tx);
        }

        _db.Transactions.AddRange(newOnes);
        await _db.SaveChangesAsync(ct);

        return newOnes.Count;
    }
}