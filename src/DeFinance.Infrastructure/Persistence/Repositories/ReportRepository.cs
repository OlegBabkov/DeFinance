using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeFinance.Infrastructure.Persistence.Repositories;

public class ReportRepository(DeFinanceDbContext db) : IReportRepository
{
    public Task<Report?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Reports.FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IReadOnlyList<Report>> GetByUserAsync(Guid userId, CancellationToken ct = default) =>
        await db.Reports
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(Report report, CancellationToken ct = default) =>
        await db.Reports.AddAsync(report, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);
}
