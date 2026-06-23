using DeFinance.Domain.Entities;

namespace DeFinance.Application.Abstractions.Repositories;

public interface IBudgetEntryRepository
{
    Task<IReadOnlyList<BudgetEntry>> GetByPeriodAsync(int year, IReadOnlyList<int> months, CancellationToken cancellationToken = default);
    Task<BudgetEntry?> GetAsync(Guid categoryId, int year, int month, CancellationToken cancellationToken = default);
    Task UpdateDirectAsync(Guid id, decimal plannedAmount, IEnumerable<(string Name, decimal Amount)> lines, CancellationToken cancellationToken = default);
    Task AddAsync(BudgetEntry entry, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
