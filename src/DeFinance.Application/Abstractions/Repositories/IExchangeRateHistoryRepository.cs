using DeFinance.Domain.Entities;

namespace DeFinance.Application.Abstractions.Repositories;

public interface IExchangeRateHistoryRepository
{
    Task<ExchangeRateHistory?> GetByCurrencyAndDateAsync(Guid currencyId, DateOnly date, CancellationToken ct = default);
    Task<List<ExchangeRateHistory>> GetLatestTwoDatesAsync(CancellationToken ct = default);
    Task<List<ExchangeRateHistory>> GetHistoryAsync(Guid currencyId, int days, CancellationToken ct = default);
    Task AddAsync(ExchangeRateHistory entity, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
