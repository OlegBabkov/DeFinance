using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Common;
using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeFinance.Infrastructure.Persistence.Repositories;

public class MandatoryPaymentRepository(DeFinanceDbContext dbContext) : IMandatoryPaymentRepository
{
    public async Task<MandatoryPayment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.MandatoryPayments
            .Include(p => p.Currency)
            .Include(p => p.Account).ThenInclude(a => a!.Currency)
            .Include(p => p.Category).ThenInclude(c => c!.Parent)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<MandatoryPayment> Items, int TotalCount)> GetAllAsync(
        string? search,
        bool? isActive,
        Guid? currencyId,
        Guid? accountId,
        Guid? categoryId,
        PaymentFrequency? frequency,
        string? sortBy,
        SortDirection sortDirection,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.MandatoryPayments
            .Include(p => p.Currency)
            .Include(p => p.Account).ThenInclude(a => a!.Currency)
            .Include(p => p.Category).ThenInclude(c => c!.Parent)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        if (currencyId.HasValue)
            query = query.Where(p => p.CurrencyId == currencyId.Value);

        if (accountId.HasValue)
            query = query.Where(p => p.AccountId == accountId.Value);

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (frequency.HasValue)
            query = query.Where(p => p.Frequency == frequency.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        query = sortBy?.ToLower() switch
        {
            "amount" => sortDirection == SortDirection.Desc
                ? query.OrderByDescending(p => p.Amount)
                : query.OrderBy(p => p.Amount),
            _ => sortDirection == SortDirection.Desc
                ? query.OrderByDescending(p => p.Name)
                : query.OrderBy(p => p.Name),
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(MandatoryPayment payment, CancellationToken cancellationToken = default) =>
        await dbContext.MandatoryPayments.AddAsync(payment, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
