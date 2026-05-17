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
        var existingNames = await context.PaymentStatuses
            .Select(p => p.Name)
            .ToHashSetAsync(cancellationToken);

        var toAdd = _statuses
            .Where(s => !existingNames.Contains(s.Name))
            .Select(s => PaymentStatus.Create(s.Name, s.Description))
            .ToList();

        if (toAdd.Count == 0) return;

        await context.PaymentStatuses.AddRangeAsync(toAdd, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
