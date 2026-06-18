using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using DeFinance.Application.Abstractions;
using DeFinance.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DeFinance.Api.Tests.Infrastructure;

public class DeFinanceWebApplicationFactory : WebApplicationFactory<Program>
{
    public static readonly Guid TestUserId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

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

            services.Replace(ServiceDescriptor.Scoped<ICurrentUserService>(_ =>
                new TestCurrentUserService(TestUserId)));

            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
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

file sealed class TestCurrentUserService(Guid userId) : ICurrentUserService
{
    public Guid UserId => userId;
}

file sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, DeFinanceWebApplicationFactory.TestUserId.ToString()),
            new Claim(ClaimTypes.Name, "test-user"),
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
