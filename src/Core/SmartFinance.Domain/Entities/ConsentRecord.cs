using SmartFinance.Domain.Common;

namespace SmartFinance.Domain.Entities;

public class ConsentRecord : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;
    public DateTime GrantedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    // Müþteri rýzasý hâlâ geçerli mi? (yasal akýþýn kalbi)
    public bool IsActive => RevokedAt is null && DateTime.UtcNow < ExpiresAt;
}