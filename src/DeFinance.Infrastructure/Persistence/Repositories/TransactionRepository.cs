using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Common;
using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeFinance.Infrastructure.Persistence.Repositories;

public class TransactionRepository(DeFinanceDbContext dbContext) : ITransactionRepository
{
    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default) =>
        await dbContext.Transactions.AddAsync(transaction, cancellationToken);

    public void Remove(Transaction transaction) =>
        dbContext.Transactions.Remove(transaction);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await dbContext.SaveChangesAsync(cancellationToken);

    public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.Transactions
            .Include(t => t.Account).ThenInclude(a => a!.Currency)
            .Include(t => t.InCurrency)
            .Include(t => t.Category).ThenInclude(c => c!.Parent)
            .Include(t => t.Counterparty)
            .Include(t => t.PaymentStatus)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<decimal?> GetBalanceBeforeAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        var tx = await dbContext.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category).ThenInclude(c => c!.Parent)
            .FirstOrDefaultAsync(t => t.Id == transactionId, cancellationToken);

        if (tx?.Account is null) return null;

        // Contributions from same-account transactions strictly after this one
        var laterAdjustment = await dbContext.Transactions
            .Where(t => t.AccountId == tx.AccountId && t.DateTime > tx.DateTime)
            .SumAsync(t =>
                t.Category!.Type == CategoryType.Income  ?  t.Sum :
                t.Category!.Type == CategoryType.Expense ? -t.Sum : 0m,
                cancellationToken);

        decimal txContribution = tx.Category?.Type switch
        {
            CategoryType.Income  =>  tx.Sum,
            CategoryType.Expense => -tx.Sum,
            _                    =>  0m,
        };

        return tx.Account.Balance - txContribution - laterAdjustment;
    }

    public async Task<(IReadOnlyList<Transaction> Items, int TotalCount, decimal TotalSum, decimal TotalAmountInCurrency)> GetAllAsync(
        DateTime? dateFrom,
        DateTime? dateTo,
        Guid? accountId,
        Guid? categoryId,
        Guid? counterpartyId,
        Guid? paymentStatusId,
        Guid? inCurrencyId,
        string? notes,
        string? sortBy,
        SortDirection sortDirection,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Transactions
            .Include(t => t.Account).ThenInclude(a => a!.Currency)
            .Include(t => t.InCurrency)
            .Include(t => t.Category).ThenInclude(c => c!.Parent)
            .Include(t => t.Counterparty)
            .Include(t => t.PaymentStatus)
            .AsQueryable();

        if (dateFrom.HasValue)
            query = query.Where(t => t.DateTime >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(t => t.DateTime < dateTo.Value);

        if (accountId.HasValue)
            query = query.Where(t => t.AccountId == accountId.Value);

        if (categoryId.HasValue)
            query = query.Where(t => t.CategoryId == categoryId.Value);

        if (counterpartyId.HasValue)
            query = query.Where(t => t.CounterpartyId == counterpartyId.Value);

        if (paymentStatusId.HasValue)
            query = query.Where(t => t.PaymentStatusId == paymentStatusId.Value);

        if (inCurrencyId.HasValue)
            query = query.Where(t => t.InCurrencyId == inCurrencyId.Value);

        if (!string.IsNullOrWhiteSpace(notes))
            query = query.Where(t => t.Notes != null && t.Notes.ToLower().Contains(notes.ToLower()));

        var totalCount = await query.CountAsync(cancellationToken);
        var totalSum = await query.SumAsync(t => t.Sum, cancellationToken);
        var totalAmountInCurrency = await query.SumAsync(t => t.AmountInCurrency, cancellationToken);

        query = sortBy?.ToLower() switch
        {
            "sum"              => sortDirection == SortDirection.Desc ? query.OrderByDescending(t => t.Sum)              : query.OrderBy(t => t.Sum),
            "amountincurrency" => sortDirection == SortDirection.Desc ? query.OrderByDescending(t => t.AmountInCurrency) : query.OrderBy(t => t.AmountInCurrency),
            "exchangerate"     => sortDirection == SortDirection.Desc ? query.OrderByDescending(t => t.ExchangeRate)     : query.OrderBy(t => t.ExchangeRate),
            _                  => sortDirection == SortDirection.Desc ? query.OrderByDescending(t => t.DateTime)         : query.OrderBy(t => t.DateTime),
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount, totalSum, totalAmountInCurrency);
    }
}
