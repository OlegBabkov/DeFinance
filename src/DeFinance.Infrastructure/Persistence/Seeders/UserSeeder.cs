using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeFinance.Infrastructure.Persistence.Seeders;

public static class UserSeeder
{
    public static async Task SeedAsync(DeFinanceDbContext context, CancellationToken cancellationToken = default)
    {
        const string adminPassword = "admin123";
        const string adminEmail = "admin@definance.local";

        var existing = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin", cancellationToken);
        if (existing is null)
        {
            var admin = User.Create(
                username: "admin",
                password: BCrypt.Net.BCrypt.HashPassword(adminPassword),
                email: adminEmail,
                phoneNumber: null);

            await context.Users.AddAsync(admin, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            var changed = false;
            if (!existing.Password.StartsWith("$2"))
            {
                existing.ChangePassword(BCrypt.Net.BCrypt.HashPassword(adminPassword));
                changed = true;
            }
            // Restore plain email if it was previously BCrypt-hashed
            if (existing.Email.StartsWith("$2"))
            {
                existing.Update(existing.Username, adminEmail, existing.PhoneNumber);
                changed = true;
            }
            if (changed)
                await context.SaveChangesAsync(cancellationToken);
        }
    }
}
