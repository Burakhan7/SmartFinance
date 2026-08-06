using Microsoft.Extensions.DependencyInjection;
using SmartFinance.Application.Common.Interfaces;
using SmartFinance.Infrastructure.Providers;

namespace SmartFinance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Mock şimdi. Gerçek HBHS gelince SADECE bu satır değişecek.
        services.AddScoped<ITransactionProvider, MockTransactionProvider>();
        return services;
    }
}