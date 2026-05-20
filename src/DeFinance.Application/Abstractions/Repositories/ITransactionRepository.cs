using DeFinance.Application.Common;
using DeFinance.Domain.Entities;

namespace DeFinance.Application.Abstractions.Repositories;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
    void Remove(Transaction transaction);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<decimal?> GetBalanceBeforeAsync(Guid transactionId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Transaction> Items, int TotalCount)> GetAllAsync(
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
        CancellationToken cancellationToken = default);
}
