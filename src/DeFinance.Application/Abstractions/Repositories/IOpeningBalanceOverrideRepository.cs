using DeFinance.Domain.Entities;

namespace DeFinance.Application.Abstractions.Repositories;

public interface IOpeningBalanceOverrideRepository
{
    Task<OpeningBalanceOverride?> GetAsync(int year, int month, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OpeningBalanceOverride>> GetByYearAsync(int year, IReadOnlyList<int> months, CancellationToken cancellationToken = default);
    Task AddAsync(OpeningBalanceOverride entry, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
