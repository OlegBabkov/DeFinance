using DeFinance.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DeFinance.Api.Tests.Infrastructure;

public class DeFinanceWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove DbContextOptions<DeFinanceDbContext> and any generic service that
            // uses DeFinanceDbContext as a type argument (catches IDbContextOptionsConfiguration<T>).
            var dbContextType = typeof(DeFinanceDbContext);
            var toRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<DeFinanceDbContext>) ||
                    d.ServiceType == dbContextType ||
                    (d.ServiceType.IsGenericType &&
                     d.ServiceType.GetGenericArguments() is { Length: 1 } args &&
                     args[0] == dbContextType))
                .ToList();

            foreach (var d in toRemove)
                services.Remove(d);

            var dbName = Guid.NewGuid().ToString();
            services.AddDbContext<DeFinanceDbContext>(options =>
                options.UseInMemoryDatabase(dbName));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DeFinanceDbContext>();
        db.Database.EnsureCreated();

        return host;
    }
}
