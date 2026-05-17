using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeFinance.Infrastructure.Persistence;

public class DeFinanceDbContext(DbContextOptions<DeFinanceDbContext> options) : DbContext(options)
{
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<Account> Accounts => Set<Account>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DeFinanceDbContext).Assembly);
    }
}
