using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeFinance.Infrastructure.Persistence.Repositories;

public class CurrencyRepository(DeFinanceDbContext dbContext) : ICurrencyRepository
{
    public async Task<Currency?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.Currencies.FindAsync([id], cancellationToken);

    public async Task<Currency?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        await dbContext.Currencies.FirstOrDefaultAsync(c => c.Code == code.ToUpperInvariant(), cancellationToken);

    public async Task<IReadOnlyList<Currency>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Currencies.ToListAsync(cancellationToken);

    public async Task AddAsync(Currency currency, CancellationToken cancellationToken = default) =>
        await dbContext.Currencies.AddAsync(currency, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
