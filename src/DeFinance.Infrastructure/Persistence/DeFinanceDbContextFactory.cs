using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DeFinance.Infrastructure.Persistence;

public class DeFinanceDbContextFactory : IDesignTimeDbContextFactory<DeFinanceDbContext>
{
    public DeFinanceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DeFinanceDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=definance;Username=definance;Password=definance_pass");

        return new DeFinanceDbContext(optionsBuilder.Options);
    }
}
