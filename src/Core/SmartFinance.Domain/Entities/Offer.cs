using SmartFinance.Domain.Common;

namespace SmartFinance.Domain.Entities;

public class Offer : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public bool IsSeen { get; set; }
}