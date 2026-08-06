using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartFinance.Application.Common.Interfaces;
using SmartFinance.Infrastructure.Persistence;
using SmartFinance.Infrastructure.Providers;

namespace SmartFinance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ITransactionProvider, MockTransactionProvider>();
        return services;
    }
}