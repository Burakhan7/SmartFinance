namespace SmartFinance.Application.Common.Interfaces;

public interface ITransactionSyncService
{
    // Dönen sayı: bu sync'te DB'ye eklenen YENİ hareket adedi
    Task<int> SyncCustomerAsync(Guid customerId, CancellationToken ct = default);
}