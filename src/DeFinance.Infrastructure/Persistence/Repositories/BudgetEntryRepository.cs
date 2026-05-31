using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeFinance.Infrastructure.Persistence.Repositories;

public class BudgetEntryRepository(DeFinanceDbContext dbContext) : IBudgetEntryRepository
{
    public async Task<IReadOnlyList<BudgetEntry>> GetByPeriodAsync(int year, IReadOnlyList<int> months, CancellationToken cancellationToken = default)
    {
        var monthList = months.ToList();
        return await dbContext.BudgetEntries
            .Include(e => e.Lines)
            .Where(e => e.Year == year && monthList.Contains(e.Month))
            .ToListAsync(cancellationToken);
    }

    public async Task<BudgetEntry?> GetAsync(Guid categoryId, int year, int month, CancellationToken cancellationToken = default) =>
        await dbContext.BudgetEntries
            .Include(e => e.Lines)
            .FirstOrDefaultAsync(e => e.CategoryId == categoryId && e.Year == year && e.Month == month, cancellationToken);

    public async Task AddAsync(BudgetEntry entry, CancellationToken cancellationToken = default) =>
        await dbContext.BudgetEntries.AddAsync(entry, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
