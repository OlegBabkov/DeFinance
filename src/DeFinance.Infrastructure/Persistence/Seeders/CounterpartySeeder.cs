using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeFinance.Infrastructure.Persistence.Seeders;

public static class CounterpartySeeder
{
    private static readonly IReadOnlyList<(string Name, CounterpartyType Type)> _counterparties =
    [
        // Supermarkets
        ("Aldi Nord",               CounterpartyType.Company),
        ("Aldi Süd",                CounterpartyType.Company),
        ("Lidl",                    CounterpartyType.Company),
        ("Rewe",                    CounterpartyType.Company),
        ("Edeka",                   CounterpartyType.Company),
        ("Kaufland",                CounterpartyType.Company),
        ("Penny",                   CounterpartyType.Company),
        ("Netto Marken-Discount",   CounterpartyType.Company),
        ("Norma",                   CounterpartyType.Company),
        ("Tegut",                   CounterpartyType.Company),
        ("Globus",                  CounterpartyType.Company),
        ("Hit",                     CounterpartyType.Company),
        ("Wasgau",                  CounterpartyType.Company),
        ("Famila",                  CounterpartyType.Company),
        ("Marktkauf",               CounterpartyType.Company),
        ("dm",                      CounterpartyType.Company),
        ("Rossmann",                CounterpartyType.Company),
        ("Müller",                  CounterpartyType.Company),

        // Gas Stations
        ("Aral - Gasoline",         CounterpartyType.Company),
        ("Aral - Cigarettes",       CounterpartyType.Company),
        ("Aral - Alcohol",          CounterpartyType.Company),
        ("Shell",                   CounterpartyType.Company),
        ("BP",                      CounterpartyType.Company),
        ("Esso",                    CounterpartyType.Company),
        ("TotalEnergies",           CounterpartyType.Company),
        ("Jet",                     CounterpartyType.Company),
        ("Agip",                    CounterpartyType.Company),
        ("OMV",                     CounterpartyType.Company),
        ("HEM",                     CounterpartyType.Company),
        ("Star",                    CounterpartyType.Company),

        // Online
        ("Amazon",                  CounterpartyType.Company),
        ("eBay",                    CounterpartyType.Company),

        // Insurance
        ("ARAG",                    CounterpartyType.Company),
        ("Allianz",                 CounterpartyType.Company),
        ("HUK-Coburg",              CounterpartyType.Company),

        // Banks
        ("Deutsche Bank",           CounterpartyType.Company),
        ("Sparkasse",               CounterpartyType.Company),
        ("Commerzbank",             CounterpartyType.Company),
        ("ING",                     CounterpartyType.Company),
        ("DKB",                     CounterpartyType.Company),
        ("Volksbank",               CounterpartyType.Company),
        ("Postbank",                CounterpartyType.Company),
        ("N26",                     CounterpartyType.Company),
        ("Comdirect",               CounterpartyType.Company),

        // Private
        ("Maria",                   CounterpartyType.Person),
        ("Mama",                    CounterpartyType.Person),
        ("Maria's Relatives",       CounterpartyType.Person),
        ("My Relatives",            CounterpartyType.Person),
    ];

    public static async Task SeedAsync(DeFinanceDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Counterparties.AnyAsync(cancellationToken))
            return;

        var toAdd = _counterparties
            .Select(c => Counterparty.Create(c.Name, c.Type, null))
            .ToList();

        await context.Counterparties.AddRangeAsync(toAdd, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
