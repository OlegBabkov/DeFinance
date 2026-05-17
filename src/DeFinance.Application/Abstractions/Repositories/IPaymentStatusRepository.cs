using DeFinance.Domain.Entities;

namespace DeFinance.Application.Abstractions.Repositories;

public interface IPaymentStatusRepository
{
    Task<PaymentStatus?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentStatus>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(PaymentStatus paymentStatus, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
