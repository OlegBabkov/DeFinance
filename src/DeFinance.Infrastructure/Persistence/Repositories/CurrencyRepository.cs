using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Common;
using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeFinance.Infrastructure.Persistence.Repositories;

public class CurrencyRepository(DeFinanceDbContext dbContext, ICacheService cache) : ICurrencyRepository
{
    public async Task<Currency?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.Currencies.FindAsync([id], cancellationToken);

    public async Task<Currency?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        await dbContext.Currencies.FirstOrDefaultAsync(c => c.Code == code.ToUpperInvariant(), cancellationToken);

    public async Task<(IReadOnlyList<Currency> Items, int TotalCount)> GetAllAsync(
        string? search,
        bool? isActive,
        string? sortBy,
        SortDirection sortDirection,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Currencies.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(term) ||
                c.Code.ToLower().Contains(term) ||
                c.Symbol.ToLower().Contains(term));
        }

        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        query = sortBy?.ToLower() switch
        {
            "code"   => sortDirection == SortDirection.Desc ? query.OrderByDescending(c => c.Code)   : query.OrderBy(c => c.Code),
            "symbol" => sortDirection == SortDirection.Desc ? query.OrderByDescending(c => c.Symbol) : query.OrderBy(c => c.Symbol),
            _        => sortDirection == SortDirection.Desc ? query.OrderByDescending(c => c.Name)   : query.OrderBy(c => c.Name),
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Currency currency, CancellationToken cancellationToken = default) =>
        await dbContext.Currencies.AddAsync(currency, cancellationToken);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await dbContext.SaveChangesAsync(cancellationToken);
        await cache.RemoveByPrefixAsync("cur:", cancellationToken);
        return result;
    }
}
