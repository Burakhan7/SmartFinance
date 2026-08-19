using SmartFinance.Application.Offers;

namespace SmartFinance.Application.Common.Interfaces;

public interface ISpendingAnalyzer
{
    Task<SpendingAnalysis> AnalyzeAsync(Guid customerId, CancellationToken ct = default);
}