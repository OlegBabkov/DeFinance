using DeFinance.Application.Common;
using DeFinance.Domain.Entities;

namespace DeFinance.Application.Abstractions.Repositories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Category> Items, int TotalCount)> GetAllAsync(
        string? search,
        bool? isActive,
        CategoryType? type,
        CategoryPaymentObligation? paymentObligation,
        string? sortBy,
        SortDirection sortDirection,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task AddAsync(Category category, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
