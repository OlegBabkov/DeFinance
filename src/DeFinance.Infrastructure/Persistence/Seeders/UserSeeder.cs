using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeFinance.Infrastructure.Persistence.Seeders;

public static class UserSeeder
{
    public static async Task SeedAsync(DeFinanceDbContext context, CancellationToken cancellationToken = default)
    {
        const string adminPassword = "admin123";

        var existing = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin", cancellationToken);
        if (existing is null)
        {
            var admin = User.Create(
                username: "admin",
                password: BCrypt.Net.BCrypt.HashPassword(adminPassword),
                email: "admin@definance.local",
                phoneNumber: null);

            await context.Users.AddAsync(admin, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
        else if (!existing.Password.StartsWith("$2"))
        {
            existing.ChangePassword(BCrypt.Net.BCrypt.HashPassword(adminPassword));
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
