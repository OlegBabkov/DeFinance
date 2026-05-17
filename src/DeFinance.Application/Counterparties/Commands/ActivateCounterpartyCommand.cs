using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.Counterparty;
using MediatR;

namespace DeFinance.Application.Counterparties.Commands;

public record ActivateCounterpartyCommand(Guid Id) : IRequest<CounterpartyResponse?>;

public class ActivateCounterpartyCommandHandler(ICounterpartyRepository repository)
    : IRequestHandler<ActivateCounterpartyCommand, CounterpartyResponse?>
{
    public async Task<CounterpartyResponse?> Handle(ActivateCounterpartyCommand request, CancellationToken cancellationToken)
    {
        var counterparty = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (counterparty is null) return null;

        counterparty.Activate();
        await repository.SaveChangesAsync(cancellationToken);
        return counterparty.ToResponse();
    }
}
