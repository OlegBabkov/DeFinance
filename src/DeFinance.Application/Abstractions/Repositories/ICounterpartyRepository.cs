using DeFinance.Domain.Entities;

namespace DeFinance.Application.Abstractions.Repositories;

public interface ICounterpartyRepository
{
    Task<Counterparty?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Counterparty>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Counterparty counterparty, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
