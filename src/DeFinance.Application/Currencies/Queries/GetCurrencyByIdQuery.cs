using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.Currency;
using MediatR;

namespace DeFinance.Application.Currencies.Queries;

public record GetCurrencyByIdQuery(Guid Id) : IRequest<CurrencyResponse?>;

public class GetCurrencyByIdQueryHandler(ICurrencyRepository repository)
    : IRequestHandler<GetCurrencyByIdQuery, CurrencyResponse?>
{
    public async Task<CurrencyResponse?> Handle(GetCurrencyByIdQuery request, CancellationToken cancellationToken)
    {
        var currency = await repository.GetByIdAsync(request.Id, cancellationToken);
        return currency?.ToResponse();
    }
}
