using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Common;
using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeFinance.Infrastructure.Persistence.Repositories;

public class AccountRepository(DeFinanceDbContext dbContext) : IAccountRepository
{
    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.Accounts
            .Include(a => a.Currency)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Account> Items, int TotalCount)> GetAllAsync(
        string? search,
        bool? isActive,
        AccountType? type,
        Guid? currencyId,
        string? sortBy,
        SortDirection sortDirection,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Accounts.Include(a => a.Currency).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(a => a.Name.ToLower().Contains(term));
        }

        if (isActive.HasValue)
            query = query.Where(a => a.IsActive == isActive.Value);

        if (type.HasValue)
            query = query.Where(a => a.Type == type.Value);

        if (currencyId.HasValue)
            query = query.Where(a => a.CurrencyId == currencyId.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        query = sortBy?.ToLower() switch
        {
            "type"    => sortDirection == SortDirection.Desc ? query.OrderByDescending(a => a.Type)    : query.OrderBy(a => a.Type),
            "balance" => sortDirection == SortDirection.Desc ? query.OrderByDescending(a => a.Balance) : query.OrderBy(a => a.Balance),
            _         => sortDirection == SortDirection.Desc ? query.OrderByDescending(a => a.Name)    : query.OrderBy(a => a.Name),
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Account account, CancellationToken cancellationToken = default) =>
        await dbContext.Accounts.AddAsync(account, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
