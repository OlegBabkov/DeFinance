using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Common;
using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeFinance.Infrastructure.Persistence.Repositories;

public class CounterpartyRepository(DeFinanceDbContext dbContext, ICacheService cache, ICurrentUserService currentUserService) : ICounterpartyRepository
{
    private readonly Guid _userId = currentUserService.UserId;

    public async Task<Counterparty?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.Counterparties
            .Where(c => c.UserId == _userId)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Counterparty> Items, int TotalCount)> GetAllAsync(
        string? search,
        bool? isActive,
        CounterpartyType? type,
        string? sortBy,
        SortDirection sortDirection,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Counterparties.Where(c => c.UserId == _userId).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(term) ||
                (c.ContactInfo != null && c.ContactInfo.ToLower().Contains(term)));
        }

        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        if (type.HasValue)
            query = query.Where(c => c.Type == type.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        query = sortBy?.ToLower() switch
        {
            "type" => sortDirection == SortDirection.Desc ? query.OrderByDescending(c => c.Type) : query.OrderBy(c => c.Type),
            _      => sortDirection == SortDirection.Desc ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Counterparty counterparty, CancellationToken cancellationToken = default) =>
        await dbContext.Counterparties.AddAsync(counterparty, cancellationToken);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await dbContext.SaveChangesAsync(cancellationToken);
        await cache.RemoveByPrefixAsync("cp:", cancellationToken);
        return result;
    }
}
