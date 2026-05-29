using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Common;
using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeFinance.Infrastructure.Persistence.Repositories;

public class PaymentStatusRepository(DeFinanceDbContext dbContext, ICacheService cache) : IPaymentStatusRepository
{
    public async Task<PaymentStatus?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.PaymentStatuses.FindAsync([id], cancellationToken);

    public async Task<(IReadOnlyList<PaymentStatus> Items, int TotalCount)> GetAllAsync(
        string? search,
        bool? isActive,
        string? sortBy,
        SortDirection sortDirection,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.PaymentStatuses.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                (p.Description != null && p.Description.ToLower().Contains(term)));
        }

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        query = sortDirection == SortDirection.Desc
            ? query.OrderByDescending(p => p.Name)
            : query.OrderBy(p => p.Name);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(PaymentStatus paymentStatus, CancellationToken cancellationToken = default) =>
        await dbContext.PaymentStatuses.AddAsync(paymentStatus, cancellationToken);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await dbContext.SaveChangesAsync(cancellationToken);
        await cache.RemoveByPrefixAsync("ps:", cancellationToken);
        return result;
    }
}
