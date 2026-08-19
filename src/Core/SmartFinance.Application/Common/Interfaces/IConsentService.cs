using SmartFinance.Domain.Entities;

namespace SmartFinance.Application.Common.Interfaces;

public interface IConsentService
{
    // Kullanıcı rıza verir (kart bağlama anı). Varsa aktif rızayı döndürür, yoksa yeni oluşturur.
    Task<ConsentRecord> GrantConsentAsync(Guid customerId, int durationDays = 90, CancellationToken ct = default);

    // Kullanıcı rızasını geri çeker
    Task RevokeConsentAsync(Guid customerId, CancellationToken ct = default);

    // Şu an aktif geçerli rıza var mı? (veri çekmeden önce bu kontrol edilecek)
    Task<bool> HasActiveConsentAsync(Guid customerId, CancellationToken ct = default);
}