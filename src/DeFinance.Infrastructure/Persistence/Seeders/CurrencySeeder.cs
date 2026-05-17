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
    ];

    public static async Task SeedAsync(DeFinanceDbContext context, CancellationToken cancellationToken = default)
    {
        var existingCodes = await context.Currencies
            .Select(c => c.Code)
            .ToHashSetAsync(cancellationToken);

        var toAdd = _currencies
            .Where(c => !existingCodes.Contains(c.Code))
            .Select(c => Currency.Create(c.Code, c.Name, c.Symbol))
            .ToList();

        if (toAdd.Count == 0)
            return;

        await context.Currencies.AddRangeAsync(toAdd, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
