using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Infrastructure.Persistence;
using DeFinance.Infrastructure.Persistence.Repositories;
using DeFinance.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace DeFinance.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DeFinanceDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        var redisConnectionString = configuration["Redis:ConnectionString"]!;
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
        services.AddStackExchangeRedisCache(opts => opts.Configuration = redisConnectionString);
        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddSingleton<IEventPublisher, RedisEventPublisher>();

        services.AddScoped<ICurrencyRepository, CurrencyRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICounterpartyRepository, CounterpartyRepository>();
        services.AddScoped<IPaymentStatusRepository, PaymentStatusRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IMandatoryPaymentRepository, MandatoryPaymentRepository>();
        services.AddScoped<IBudgetEntryRepository, BudgetEntryRepository>();

        services.AddScoped<IPasswordService, BCryptPasswordService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }
}
