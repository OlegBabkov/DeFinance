using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeFinance.Infrastructure.Persistence.Seeders;

public static class PaymentStatusSeeder
{
    private static readonly IReadOnlyList<(string Name, string Description)> _statuses =
    [
        ("Paid",     "Transaction has been fully paid."),
        ("Rejected", "Transaction was declined or rejected."),
        ("Reserved", "Funds are reserved but not yet settled."),
        ("Booked",   "Transaction is confirmed and booked."),
    ];

    public static async Task SeedAsync(DeFinanceDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.PaymentStatuses.AnyAsync(cancellationToken))
            return;

        var toAdd = _statuses
            .Select(s => PaymentStatus.Create(s.Name, s.Description))
            .ToList();

        await context.PaymentStatuses.AddRangeAsync(toAdd, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
