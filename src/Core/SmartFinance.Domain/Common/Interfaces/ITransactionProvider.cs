using SmartFinance.Domain.Entities;

namespace SmartFinance.Application.Common.Interfaces;

public interface ITransactionProvider
{
     Task<IReadOnlyList<Transaction>> GetTransactionsAsync(
        Guid customerId,
        DateTime from,
        DateTime to,
        CancellationToken ct = default);
}