using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.ExchangeRate;
using MediatR;

namespace DeFinance.Application.ExchangeRates.Queries;

public record GetLatestExchangeRatesQuery : IRequest<IReadOnlyList<ExchangeRateLatestDto>>;

public class GetLatestExchangeRatesQueryHandler(IExchangeRateHistoryRepository repository)
    : IRequestHandler<GetLatestExchangeRatesQuery, IReadOnlyList<ExchangeRateLatestDto>>
{
    public async Task<IReadOnlyList<ExchangeRateLatestDto>> Handle(GetLatestExchangeRatesQuery request, CancellationToken cancellationToken)
    {
        var records = await repository.GetLatestTwoDatesAsync(cancellationToken);

        var byDate = records
            .GroupBy(r => r.Date)
            .OrderByDescending(g => g.Key)
            .ToList();

        var latest = byDate.ElementAtOrDefault(0)?.ToDictionary(r => r.CurrencyId) ?? [];
        var previous = byDate.ElementAtOrDefault(1)?.ToDictionary(r => r.CurrencyId) ?? [];

        return latest.Values
            .Select(r => new ExchangeRateLatestDto(
                r.CurrencyId,
                r.Currency!.Code,
                r.Rate,
                previous.TryGetValue(r.CurrencyId, out var prev) ? prev.Rate : null,
                r.Date))
            .OrderBy(r => r.CurrencyCode)
            .ToList();
    }
}
