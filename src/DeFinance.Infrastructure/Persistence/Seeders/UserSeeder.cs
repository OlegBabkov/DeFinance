using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeFinance.Infrastructure.Persistence.Seeders;

public static class UserSeeder
{
    public static async Task SeedAsync(DeFinanceDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Users.AnyAsync(cancellationToken))
            return;

        var admin = User.Create(
            username: "admin",
            password: "admin",
            email: "admin@definance.local",
            phoneNumber: "+1 (555) 000-0001");

        await context.Users.AddAsync(admin, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
