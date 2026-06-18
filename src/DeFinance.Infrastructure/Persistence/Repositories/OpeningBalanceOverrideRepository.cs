using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeFinance.Infrastructure.Persistence.Repositories;

public class OpeningBalanceOverrideRepository(DeFinanceDbContext dbContext, ICurrentUserService currentUserService) : IOpeningBalanceOverrideRepository
{
    private readonly Guid _userId = currentUserService.UserId;

    public async Task<OpeningBalanceOverride?> GetAsync(int year, int month, CancellationToken cancellationToken = default) =>
        await dbContext.OpeningBalanceOverrides
            .FirstOrDefaultAsync(e => e.UserId == _userId && e.Year == year && e.Month == month, cancellationToken);

    public async Task<IReadOnlyList<OpeningBalanceOverride>> GetByYearAsync(int year, IReadOnlyList<int> months, CancellationToken cancellationToken = default)
    {
        var monthList = months.ToList();
        return await dbContext.OpeningBalanceOverrides
            .Where(e => e.UserId == _userId && e.Year == year && monthList.Contains(e.Month))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(OpeningBalanceOverride entry, CancellationToken cancellationToken = default) =>
        await dbContext.OpeningBalanceOverrides.AddAsync(entry, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
