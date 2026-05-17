using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeFinance.Infrastructure.Persistence.Repositories;

public class PaymentStatusRepository(DeFinanceDbContext dbContext) : IPaymentStatusRepository
{
    public async Task<PaymentStatus?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.PaymentStatuses.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<PaymentStatus>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.PaymentStatuses.ToListAsync(cancellationToken);

    public async Task AddAsync(PaymentStatus paymentStatus, CancellationToken cancellationToken = default) =>
        await dbContext.PaymentStatuses.AddAsync(paymentStatus, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
