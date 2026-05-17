using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.Counterparty;
using MediatR;

namespace DeFinance.Application.Counterparties.Queries;

public record GetCounterpartyByIdQuery(Guid Id) : IRequest<CounterpartyResponse?>;

public class GetCounterpartyByIdQueryHandler(ICounterpartyRepository repository)
    : IRequestHandler<GetCounterpartyByIdQuery, CounterpartyResponse?>
{
    public async Task<CounterpartyResponse?> Handle(GetCounterpartyByIdQuery request, CancellationToken cancellationToken)
    {
        var counterparty = await repository.GetByIdAsync(request.Id, cancellationToken);
        return counterparty?.ToResponse();
    }
}
