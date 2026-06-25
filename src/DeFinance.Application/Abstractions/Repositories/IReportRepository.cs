using DeFinance.Domain.Entities;

namespace DeFinance.Application.Abstractions.Repositories;

public interface IReportRepository
{
    Task<Report?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Report>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(Report report, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
