using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeFinance.Infrastructure.Persistence.Repositories;

public class CounterpartyRepository(DeFinanceDbContext dbContext) : ICounterpartyRepository
{
    public async Task<Counterparty?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.Counterparties.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<Counterparty>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Counterparties.ToListAsync(cancellationToken);

    public async Task AddAsync(Counterparty counterparty, CancellationToken cancellationToken = default) =>
        await dbContext.Counterparties.AddAsync(counterparty, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
