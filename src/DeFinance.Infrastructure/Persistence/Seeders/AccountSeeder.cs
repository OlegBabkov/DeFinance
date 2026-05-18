using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeFinance.Infrastructure.Persistence.Seeders;

public static class AccountSeeder
{
    private static readonly IReadOnlyList<(string Name, AccountType Type, string CurrencyCode)> _accounts =
    [
        ("PrivatBank (UAH)", AccountType.Checking, "UAH"),
        ("PrivatBank",       AccountType.Checking, "EUR"),
        ("DeutscheBank",     AccountType.Checking, "EUR"),
        ("Monobank (UAH)",   AccountType.Checking, "UAH"),
        ("Monobank",         AccountType.Checking, "EUR"),
        ("C24",              AccountType.Checking, "EUR"),
        ("Cash",             AccountType.Cash,     "EUR"),
        ("Family Fund",      AccountType.Savings,  "EUR"),
        ("Family Fund Cash", AccountType.Cash,     "EUR"),
        ("Vacation Fund",    AccountType.Savings,  "EUR"),
    ];

    public static async Task SeedAsync(DeFinanceDbContext context, CancellationToken cancellationToken = default)
    {
        var existingNames = await context.Accounts
            .Select(a => a.Name)
            .ToHashSetAsync(cancellationToken);

        var currencies = await context.Currencies
            .Where(c => new[] { "UAH", "EUR" }.Contains(c.Code))
            .ToDictionaryAsync(c => c.Code, c => c.Id, cancellationToken);

        var toAdd = _accounts
            .Where(a => !existingNames.Contains(a.Name) && currencies.ContainsKey(a.CurrencyCode))
            .Select(a => Account.Create(a.Name, a.Type, 0m, currencies[a.CurrencyCode]))
            .ToList();

        if (toAdd.Count == 0) return;

        await context.Accounts.AddRangeAsync(toAdd, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
