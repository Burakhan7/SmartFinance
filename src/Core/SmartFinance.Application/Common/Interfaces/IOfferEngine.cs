using SmartFinance.Domain.Entities;

namespace SmartFinance.Application.Common.Interfaces;

public interface IOfferEngine
{
    // Analiz et -> fırsat üret -> DB'ye yaz. Dönen: üretilen yeni fırsatlar.
    Task<IReadOnlyList<Offer>> GenerateOffersAsync(Guid customerId, CancellationToken ct = default);
}