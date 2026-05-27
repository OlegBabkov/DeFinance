using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeFinance.Infrastructure.Persistence.Seeders;

public static class CurrencySeeder
{
    private static readonly IReadOnlyList<(string Code, string Name, string Symbol)> _currencies =
    [
        ("USD", "US Dollar", "$"),
        ("EUR", "Euro", "€"),
        ("GBP", "British Pound", "£"),
        ("JPY", "Japanese Yen", "¥"),
        ("CHF", "Swiss Franc", "Fr"),
        ("CAD", "Canadian Dollar", "C$"),
        ("AUD", "Australian Dollar", "A$"),
        ("CNY", "Chinese Yuan", "¥"),
        ("SEK", "Swedish Krona", "kr"),
        ("NOK", "Norwegian Krone", "kr"),
        ("PLN", "Polish Złoty", "zł"),
        ("UAH", "Ukrainian Hryvnia", "₴"),
        ("CZK", "Czech Koruna",      "Kč"),
    ];

    public static async Task SeedAsync(DeFinanceDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Currencies.AnyAsync(cancellationToken))
            return;

        var toAdd = _currencies
            .Select(c => Currency.Create(c.Code, c.Name, c.Symbol))
            .ToList();

        await context.Currencies.AddRangeAsync(toAdd, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
