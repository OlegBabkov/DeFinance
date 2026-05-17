using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.Currency;
using MediatR;

namespace DeFinance.Application.Currencies.Commands;

public record ActivateCurrencyCommand(Guid Id) : IRequest<CurrencyResponse?>;

public class ActivateCurrencyCommandHandler(ICurrencyRepository repository)
    : IRequestHandler<ActivateCurrencyCommand, CurrencyResponse?>
{
    public async Task<CurrencyResponse?> Handle(ActivateCurrencyCommand request, CancellationToken cancellationToken)
    {
        var currency = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (currency is null) return null;

        currency.Activate();
        await repository.SaveChangesAsync(cancellationToken);
        return currency.ToResponse();
    }
}
