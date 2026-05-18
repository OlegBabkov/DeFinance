using DeFinance.Application.Common;
using DeFinance.Domain.Entities;

namespace DeFinance.Application.Abstractions.Repositories;

public interface ICounterpartyRepository
{
    Task<Counterparty?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Counterparty> Items, int TotalCount)> GetAllAsync(
        string? search,
        bool? isActive,
        CounterpartyType? type,
        string? sortBy,
        SortDirection sortDirection,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task AddAsync(Counterparty counterparty, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
