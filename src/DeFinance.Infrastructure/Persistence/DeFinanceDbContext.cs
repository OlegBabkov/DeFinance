using DeFinance.Application.Abstractions;
using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeFinance.Infrastructure.Persistence;

public class DeFinanceDbContext(DbContextOptions<DeFinanceDbContext> options, ICurrentUserService currentUserService) : DbContext(options)
{
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Counterparty> Counterparties => Set<Counterparty>();
    public DbSet<PaymentStatus> PaymentStatuses => Set<PaymentStatus>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<MandatoryPayment> MandatoryPayments => Set<MandatoryPayment>();
    public DbSet<BudgetEntry> BudgetEntries => Set<BudgetEntry>();
    public DbSet<BudgetEntryLine> BudgetEntryLines => Set<BudgetEntryLine>();
    public DbSet<OpeningBalanceOverride> OpeningBalanceOverrides => Set<OpeningBalanceOverride>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DeFinanceDbContext).Assembly);

        modelBuilder.Entity<Account>().HasQueryFilter(a => !currentUserService.UserId.HasValue || a.UserId == currentUserService.UserId.GetValueOrDefault());
        modelBuilder.Entity<Category>().HasQueryFilter(c => !currentUserService.UserId.HasValue || c.UserId == currentUserService.UserId.GetValueOrDefault());
        modelBuilder.Entity<Transaction>().HasQueryFilter(t => !currentUserService.UserId.HasValue || t.UserId == currentUserService.UserId.GetValueOrDefault());
        modelBuilder.Entity<Counterparty>().HasQueryFilter(c => !currentUserService.UserId.HasValue || c.UserId == currentUserService.UserId.GetValueOrDefault());
        modelBuilder.Entity<MandatoryPayment>().HasQueryFilter(m => !currentUserService.UserId.HasValue || m.UserId == currentUserService.UserId.GetValueOrDefault());
        modelBuilder.Entity<BudgetEntry>().HasQueryFilter(b => !currentUserService.UserId.HasValue || b.UserId == currentUserService.UserId.GetValueOrDefault());
        modelBuilder.Entity<OpeningBalanceOverride>().HasQueryFilter(o => !currentUserService.UserId.HasValue || o.UserId == currentUserService.UserId.GetValueOrDefault());
    }
}
