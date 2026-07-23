using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Domain.Entities;
using MediatR;

namespace DeFinance.Application.ExchangeRates.Commands;

public record SyncExchangeRatesCommand : IRequest<int>;

public class SyncExchangeRatesCommandHandler(
    ICurrencyRepository currencyRepository,
    IExchangeRateHistoryRepository historyRepository,
    IFrankfurterService frankfurter,
    INbuService nbu) : IRequestHandler<SyncExchangeRatesCommand, int>
{
    public async Task<int> Handle(SyncExchangeRatesCommand request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var currencies = await currencyRepository.GetAllActiveAsync(cancellationToken);
        var currencyByCode = currencies
            .Where(c => !c.Code.Equals("EUR", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(c => c.Code, c => c);

        // Frankfurter (ECB) — covers most currencies except UAH
        var frankfurterResponse = await frankfurter.GetLatestAsync(cancellationToken);
        var rates = frankfurterResponse?.Rates ?? [];

        // NBU — official UAH rate (ECB does not publish UAH)
        if (currencyByCode.ContainsKey("UAH"))
        {
            var uahRate = await nbu.GetUahPerEurAsync(cancellationToken);
            if (uahRate.HasValue)
                rates["UAH"] = uahRate.Value;
        }

        var synced = 0;
        foreach (var (code, rate) in rates)
        {
            if (!currencyByCode.TryGetValue(code, out var currency)) continue;

            var existing = await historyRepository.GetByCurrencyAndDateAsync(currency.Id, today, cancellationToken);
            if (existing is not null)
                existing.UpdateRate(rate);
            else
                await historyRepository.AddAsync(ExchangeRateHistory.Create(currency.Id, today, rate), cancellationToken);

            synced++;
        }

        if (synced > 0)
            await historyRepository.SaveChangesAsync(cancellationToken);

        return synced;
    }
}
