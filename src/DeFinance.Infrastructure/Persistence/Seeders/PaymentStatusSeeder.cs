using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeFinance.Infrastructure.Persistence.Seeders;

public static class PaymentStatusSeeder
{
    private static readonly IReadOnlyList<(string Name, string Description, bool AffectsBalance)> _statuses =
    [
        ("Paid",     "Transaction has been fully paid.",        true),
        ("Rejected", "Transaction was declined or rejected.",   false),
        ("Reserved", "Funds are reserved but not yet settled.", true),
        ("Booked",   "Transaction is confirmed and booked.",    true),
    ];

    public static async Task SeedAsync(DeFinanceDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.PaymentStatuses.AnyAsync(cancellationToken))
            return;

        var toAdd = _statuses
            .Select(s => PaymentStatus.Create(s.Name, s.Description, affectsBalance: s.AffectsBalance))
            .ToList();

        await context.PaymentStatuses.AddRangeAsync(toAdd, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
