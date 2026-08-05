using SmartFinance.Domain.Common;
using SmartFinance.Domain.Enums;

namespace SmartFinance.Domain.Entities;

public class Card : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;
    public string MaskedNumber { get; set; } = default!; // ör. "**** **** **** 1234"
    public CardType Type { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}