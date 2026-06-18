using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeFinance.Infrastructure.Persistence.Seeders;

public static class TransactionSeeder
{
    private static readonly (decimal Sum, string Category, string? Notes, int DaysAgo)[] _data =
    [
        (3200m,  "Salary",            "March salary",            365),
        (3200m,  "Salary",            "April salary",            335),
        (3200m,  "Salary",            "May salary",              305),
        (3200m,  "Salary",            "June salary",             275),
        (3200m,  "Salary",            "July salary",             245),
        (3200m,  "Salary",            "August salary",           215),
        (3200m,  "Salary",            "September salary",        185),
        (3200m,  "Salary",            "October salary",          155),
        (3200m,  "Salary",            "November salary",         125),
        (3200m,  "Salary",            "December salary",          95),
        (3200m,  "Salary",            "January salary",           65),
        (3200m,  "Salary",            "February salary",          35),
        (800m,   "Apartment Rent",    "Monthly rent",            360),
        (800m,   "Apartment Rent",    "Monthly rent",            330),
        (800m,   "Apartment Rent",    "Monthly rent",            300),
        (800m,   "Apartment Rent",    "Monthly rent",            270),
        (800m,   "Apartment Rent",    "Monthly rent",            240),
        (800m,   "Apartment Rent",    "Monthly rent",            210),
        (800m,   "Apartment Rent",    "Monthly rent",            180),
        (800m,   "Apartment Rent",    "Monthly rent",            150),
        (800m,   "Apartment Rent",    "Monthly rent",            120),
        (800m,   "Apartment Rent",    "Monthly rent",             90),
        (800m,   "Apartment Rent",    "Monthly rent",             60),
        (800m,   "Apartment Rent",    "Monthly rent",             30),
        (142m,   "Groceries",         "Weekly groceries",        358),
        (118m,   "Groceries",         "Weekly groceries",        351),
        (134m,   "Groceries",         "Weekly groceries",        344),
        (97m,    "Groceries",         null,                      337),
        (156m,   "Groceries",         "Weekend shop",            280),
        (89m,    "Groceries",         null,                      200),
        (112m,   "Groceries",         "Monthly top-up",          140),
        (78m,    "Groceries",         null,                       70),
        (45m,    "Restaurants",       "Dinner with friends",     320),
        (32m,    "Coffee & Cafes",    null,                      310),
        (67m,    "Restaurants",       "Birthday dinner",         260),
        (18m,    "Coffee & Cafes",    null,                      220),
        (54m,    "Restaurants",       null,                      160),
        (24m,    "Coffee & Cafes",    null,                      100),
        (38m,    "Restaurants",       "Lunch",                    50),
        (49m,    "Utilities",         "Electricity + water",     355),
        (52m,    "Utilities",         "Electricity + water",     295),
        (47m,    "Utilities",         "Electricity + water",     235),
        (55m,    "Utilities",         "Electricity + water",     175),
        (50m,    "Utilities",         "Electricity + water",     115),
        (53m,    "Utilities",         "Electricity + water",      55),
        (29m,    "Internet & Phone",  "Internet + mobile",       350),
        (29m,    "Internet & Phone",  "Internet + mobile",       290),
        (240m,   "Clothing",          "Winter jacket",           230),
        (85m,    "Pharmacy",          "Vitamins and meds",       170),
        (1200m,  "Flights",           "Summer vacation flights", 170),
    ];

    public static async Task SeedAsync(DeFinanceDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Transactions.IgnoreQueryFilters().AnyAsync(cancellationToken))
            return;

        var admin = await context.Users.FirstOrDefaultAsync(cancellationToken);
        if (admin is null) return;

        var eurId = await context.Currencies
            .Where(c => c.Code == "EUR")
            .Select(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (eurId == default) return;

        var eurAccountIds = await context.Accounts
            .IgnoreQueryFilters()
            .Where(a => a.Currency != null && a.Currency.Code == "EUR" && a.IsActive)
            .Select(a => a.Id)
            .ToListAsync(cancellationToken);

        if (eurAccountIds.Count == 0) return;

        var categories = await context.Categories
            .IgnoreQueryFilters()
            .Select(c => new { c.Id, c.Name })
            .ToListAsync(cancellationToken);

        var categoryByName = categories.ToDictionary(c => c.Name, c => c.Id);

        var counterpartyIds = await context.Counterparties
            .IgnoreQueryFilters()
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var paymentStatusIds = await context.PaymentStatuses
            .Select(s => new { s.Id, s.Name })
            .ToDictionaryAsync(s => s.Name, s => s.Id, cancellationToken);

        if (paymentStatusIds.Count == 0) return;

        var paidId   = paymentStatusIds.GetValueOrDefault("Paid",   paymentStatusIds.Values.First());
        var bookedId = paymentStatusIds.GetValueOrDefault("Booked", paidId);

        var rng = new Random(42);
        var now = DateTime.UtcNow;
        var transactions = new List<Transaction>();

        foreach (var (sum, categoryName, notes, daysAgo) in _data)
        {
            if (!categoryByName.TryGetValue(categoryName, out var categoryId))
                continue;

            var accountId = eurAccountIds[rng.Next(eurAccountIds.Count)];

            Guid? counterpartyId = counterpartyIds.Count > 0 && rng.Next(3) > 0
                ? counterpartyIds[rng.Next(counterpartyIds.Count)]
                : null;

            var statusId = categoryName == "Salary" ? bookedId : paidId;

            var dt = now.AddDays(-daysAgo).AddHours(rng.Next(8, 20)).AddMinutes(rng.Next(0, 60));

            transactions.Add(Transaction.Create(
                dt, sum, 1m, eurId, accountId, categoryId, counterpartyId, statusId, admin.Id, notes));
        }

        await context.Transactions.AddRangeAsync(transactions, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
