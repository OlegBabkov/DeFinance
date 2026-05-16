using DeFinance.Domain.Entities;

namespace DeFinance.Application.Abstractions.Repositories;

public interface ICurrencyRepository
{
    Task<Currency?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Currency?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Currency>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Currency currency, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
