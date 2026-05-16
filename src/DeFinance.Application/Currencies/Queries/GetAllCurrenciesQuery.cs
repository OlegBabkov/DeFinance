using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.Currency;
using MediatR;

namespace DeFinance.Application.Currencies.Queries;

public record GetAllCurrenciesQuery : IRequest<IReadOnlyList<CurrencyResponse>>;

public class GetAllCurrenciesQueryHandler(ICurrencyRepository repository)
    : IRequestHandler<GetAllCurrenciesQuery, IReadOnlyList<CurrencyResponse>>
{
    public async Task<IReadOnlyList<CurrencyResponse>> Handle(GetAllCurrenciesQuery request, CancellationToken cancellationToken) =>
        (await repository.GetAllAsync(cancellationToken)).ToResponse();
}
