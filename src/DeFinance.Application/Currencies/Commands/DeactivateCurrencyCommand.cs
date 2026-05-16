using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.Currency;
using MediatR;

namespace DeFinance.Application.Currencies.Commands;

public record DeactivateCurrencyCommand(Guid Id) : IRequest<CurrencyResponse?>;

public class DeactivateCurrencyCommandHandler(ICurrencyRepository repository)
    : IRequestHandler<DeactivateCurrencyCommand, CurrencyResponse?>
{
    public async Task<CurrencyResponse?> Handle(DeactivateCurrencyCommand request, CancellationToken cancellationToken)
    {
        var currency = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (currency is null) return null;

        currency.Deactivate();
        await repository.SaveChangesAsync(cancellationToken);
        return currency.ToResponse();
    }
}
