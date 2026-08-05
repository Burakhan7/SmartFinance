using SmartFinance.Domain.Common;

namespace SmartFinance.Domain.Entities;

public class Customer : BaseEntity
{
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;

    public ICollection<Card> Cards { get; set; } = new List<Card>();
    public ICollection<Offer> Offers { get; set; } = new List<Offer>();
}