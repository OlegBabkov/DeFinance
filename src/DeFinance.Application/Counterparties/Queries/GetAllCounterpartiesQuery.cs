using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.Counterparty;
using MediatR;

namespace DeFinance.Application.Counterparties.Queries;

public record GetAllCounterpartiesQuery : IRequest<IReadOnlyList<CounterpartyResponse>>;

public class GetAllCounterpartiesQueryHandler(ICounterpartyRepository repository)
    : IRequestHandler<GetAllCounterpartiesQuery, IReadOnlyList<CounterpartyResponse>>
{
    public async Task<IReadOnlyList<CounterpartyResponse>> Handle(GetAllCounterpartiesQuery request, CancellationToken cancellationToken) =>
        (await repository.GetAllAsync(cancellationToken)).ToResponse();
}
