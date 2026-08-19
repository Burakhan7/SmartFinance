using Microsoft.EntityFrameworkCore;
using SmartFinance.Application.Common.Interfaces;
using SmartFinance.Domain.Entities;
using SmartFinance.Infrastructure.Persistence;

namespace SmartFinance.Infrastructure.Consent;

public class ConsentService : IConsentService
{
    private readonly AppDbContext _db;
    public ConsentService(AppDbContext db) => _db = db;

    public async Task<ConsentRecord> GrantConsentAsync(
        Guid customerId, int durationDays = 90, CancellationToken ct = default)
    {
        // Müşteri yoksa oluştur (gerçekte kayıt akışından gelir)
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == customerId, ct);
        if (customer is null)
        {
            customer = new Customer
            {
                Id = customerId,
                FullName = "Test Kullanıcı",
                Email = $"user-{customerId:N}@smartfinance.local"
            };
            _db.Customers.Add(customer);
        }

        // Zaten aktif rıza varsa onu döndür (mükerrer rıza oluşturma)
        var existing = await _db.ConsentRecords
            .Where(c => c.CustomerId == customerId)
            .ToListAsync(ct);

        var active = existing.FirstOrDefault(c => c.IsActive);
        if (active is not null) return active;

        // Yeni rıza oluştur
        var consent = new ConsentRecord
        {
            CustomerId = customerId,
            GrantedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(durationDays),
            RevokedAt = null
        };
        _db.ConsentRecords.Add(consent);
        await _db.SaveChangesAsync(ct);
        return consent;
    }

    public async Task RevokeConsentAsync(Guid customerId, CancellationToken ct = default)
    {
        var activeConsents = await _db.ConsentRecords
            .Where(c => c.CustomerId == customerId && c.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var c in activeConsents)
            c.RevokedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> HasActiveConsentAsync(Guid customerId, CancellationToken ct = default)
    {
        var consents = await _db.ConsentRecords
            .Where(c => c.CustomerId == customerId)
            .ToListAsync(ct);

        return consents.Any(c => c.IsActive);
    }
}