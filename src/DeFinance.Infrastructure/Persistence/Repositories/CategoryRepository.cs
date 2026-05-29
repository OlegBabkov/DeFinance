using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Common;
using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeFinance.Infrastructure.Persistence.Repositories;

public class CategoryRepository(DeFinanceDbContext dbContext, ICacheService cache) : ICategoryRepository
{
    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.Categories.FindAsync([id], cancellationToken);

    public async Task<(IReadOnlyList<Category> Items, int TotalCount)> GetAllAsync(
        string? search,
        bool? isActive,
        CategoryType? type,
        CategoryPaymentObligation? paymentObligation,
        string? sortBy,
        SortDirection sortDirection,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Categories.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(term));
        }

        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        if (type.HasValue)
            query = query.Where(c => c.Type == type.Value);

        if (paymentObligation.HasValue)
            query = query.Where(c => c.PaymentObligation == paymentObligation.Value);

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

    public async Task<IReadOnlyList<Category>> GetActiveByTypesAsync(IReadOnlyList<CategoryType> types, CancellationToken cancellationToken = default)
    {
        var typeList = types.ToList();
        return await dbContext.Categories
            .Where(c => c.IsActive && typeList.Contains(c.Type))
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default) =>
        await dbContext.Categories.AddAsync(category, cancellationToken);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await dbContext.SaveChangesAsync(cancellationToken);
        await cache.RemoveByPrefixAsync("cat:", cancellationToken);
        return result;
    }
}
