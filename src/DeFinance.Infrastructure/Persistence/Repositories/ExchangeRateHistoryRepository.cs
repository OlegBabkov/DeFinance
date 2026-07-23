using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeFinance.Infrastructure.Persistence.Repositories;

public class ExchangeRateHistoryRepository(DeFinanceDbContext dbContext) : IExchangeRateHistoryRepository
{
    public async Task<ExchangeRateHistory?> GetByCurrencyAndDateAsync(Guid currencyId, DateOnly date, CancellationToken ct = default) =>
        await dbContext.ExchangeRateHistories
            .FirstOrDefaultAsync(e => e.CurrencyId == currencyId && e.Date == date, ct);

    public async Task<List<ExchangeRateHistory>> GetLatestTwoDatesAsync(CancellationToken ct = default)
    {
        var topDates = await dbContext.ExchangeRateHistories
            .Select(e => e.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .Take(2)
            .ToListAsync(ct);

        if (topDates.Count == 0) return [];

        return await dbContext.ExchangeRateHistories
            .Where(e => topDates.Contains(e.Date))
            .Include(e => e.Currency)
            .ToListAsync(ct);
    }

    public async Task<List<ExchangeRateHistory>> GetHistoryAsync(Guid currencyId, int days, CancellationToken ct = default)
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-days));
        return await dbContext.ExchangeRateHistories
            .Where(e => e.CurrencyId == currencyId && e.Date >= from)
            .OrderByDescending(e => e.Date)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ExchangeRateHistory entity, CancellationToken ct = default) =>
        await dbContext.ExchangeRateHistories.AddAsync(entity, ct);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await dbContext.SaveChangesAsync(ct);
}
