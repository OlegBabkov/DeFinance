using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.Counterparty;
using MediatR;

namespace DeFinance.Application.Counterparties.Commands;

public record DeactivateCounterpartyCommand(Guid Id) : IRequest<CounterpartyResponse?>;

public class DeactivateCounterpartyCommandHandler(ICounterpartyRepository repository)
    : IRequestHandler<DeactivateCounterpartyCommand, CounterpartyResponse?>
{
    public async Task<CounterpartyResponse?> Handle(DeactivateCounterpartyCommand request, CancellationToken cancellationToken)
    {
        var counterparty = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (counterparty is null) return null;

        counterparty.Deactivate();
        await repository.SaveChangesAsync(cancellationToken);
        return counterparty.ToResponse();
    }
}
