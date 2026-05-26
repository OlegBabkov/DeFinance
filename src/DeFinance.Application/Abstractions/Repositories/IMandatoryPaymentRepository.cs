using DeFinance.Application.Common;
using DeFinance.Domain.Entities;

namespace DeFinance.Application.Abstractions.Repositories;

public interface IMandatoryPaymentRepository
{
    Task<MandatoryPayment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<MandatoryPayment> Items, int TotalCount)> GetAllAsync(
        string? search,
        bool? isActive,
        Guid? currencyId,
        Guid? accountId,
        Guid? categoryId,
        Guid? paymentStatusId,
        PaymentFrequency? frequency,
        string? sortBy,
        SortDirection sortDirection,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task AddAsync(MandatoryPayment payment, CancellationToken cancellationToken = default);
    Task<int> ResetPaymentStatusesByAccountAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
