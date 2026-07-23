using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.ExchangeRate;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.ExchangeRates.Queries;

public record GetExchangeRateHistoryQuery(Guid CurrencyId, int Days = 30) : IRequest<IReadOnlyList<ExchangeRateHistoryDto>>;

public class GetExchangeRateHistoryQueryHandler(IExchangeRateHistoryRepository repository)
    : IRequestHandler<GetExchangeRateHistoryQuery, IReadOnlyList<ExchangeRateHistoryDto>>
{
    public async Task<IReadOnlyList<ExchangeRateHistoryDto>> Handle(GetExchangeRateHistoryQuery request, CancellationToken cancellationToken)
    {
        var records = await repository.GetHistoryAsync(request.CurrencyId, request.Days, cancellationToken);
        return records.Select(r => new ExchangeRateHistoryDto(r.Date, r.Rate)).ToList();
    }
}

public class GetExchangeRateHistoryQueryValidator : AbstractValidator<GetExchangeRateHistoryQuery>
{
    public GetExchangeRateHistoryQueryValidator()
    {
        RuleFor(x => x.Days).InclusiveBetween(1, 365);
    }
}
