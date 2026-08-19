using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartFinance.Application.Common.Interfaces;
using SmartFinance.Infrastructure.Persistence;
using SmartFinance.Infrastructure.Providers;
using SmartFinance.Infrastructure.Sync;

namespace SmartFinance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ITransactionProvider, MockTransactionProvider>();
        services.AddScoped<ITransactionSyncService, TransactionSyncService>();
        return services;
    }
}