using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Infrastructure.Persistence;
using DeFinance.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeFinance.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DeFinanceDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ICurrencyRepository, CurrencyRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();

        return services;
    }
}
