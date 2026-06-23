using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeFinance.Infrastructure.Persistence.Repositories;

public class BudgetEntryRepository(DeFinanceDbContext dbContext, ICurrentUserService currentUserService) : IBudgetEntryRepository
{
    private readonly Guid _userId = currentUserService.UserId;

    public async Task<IReadOnlyList<BudgetEntry>> GetByPeriodAsync(int year, IReadOnlyList<int> months, CancellationToken cancellationToken = default)
    {
        var monthList = months.ToList();
        return await dbContext.BudgetEntries
            .Where(e => e.UserId == _userId)
            .Include(e => e.Lines)
            .Where(e => e.Year == year && monthList.Contains(e.Month))
            .ToListAsync(cancellationToken);
    }

    public async Task<BudgetEntry?> GetAsync(Guid categoryId, int year, int month, CancellationToken cancellationToken = default) =>
        await dbContext.BudgetEntries
            .Where(e => e.UserId == _userId)
            .FirstOrDefaultAsync(e => e.CategoryId == categoryId && e.Year == year && e.Month == month, cancellationToken);

    public async Task UpdateDirectAsync(Guid id, decimal plannedAmount, IEnumerable<(string Name, decimal Amount)> lines, CancellationToken cancellationToken = default)
    {
        await dbContext.BudgetEntries
            .Where(e => e.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.PlannedAmount, plannedAmount), cancellationToken);

        await dbContext.BudgetEntryLines
            .Where(l => l.BudgetEntryId == id)
            .ExecuteDeleteAsync(cancellationToken);

        var newLines = lines.Select((l, i) => BudgetEntryLine.Create(id, l.Name, l.Amount, i)).ToList();
        if (newLines.Count > 0)
        {
            await dbContext.BudgetEntryLines.AddRangeAsync(newLines, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task AddAsync(BudgetEntry entry, CancellationToken cancellationToken = default) =>
        await dbContext.BudgetEntries.AddAsync(entry, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
