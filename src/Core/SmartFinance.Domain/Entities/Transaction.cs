using SmartFinance.Domain.Common;
using SmartFinance.Domain.Enums;

namespace SmartFinance.Domain.Entities;

public class Transaction : BaseEntity
{
    public Guid CardId { get; set; }
    public Card Card { get; set; } = default!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public string MerchantName { get; set; } = default!;
    public TransactionCategory Category { get; set; }
    public TransactionDirection Direction { get; set; }
    public DateTime OccurredAt { get; set; }
}