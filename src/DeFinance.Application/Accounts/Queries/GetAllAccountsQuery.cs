using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.Account;
using MediatR;

namespace DeFinance.Application.Accounts.Queries;

public record GetAllAccountsQuery : IRequest<IReadOnlyList<AccountResponse>>;

public class GetAllAccountsQueryHandler(IAccountRepository repository)
    : IRequestHandler<GetAllAccountsQuery, IReadOnlyList<AccountResponse>>
{
    public async Task<IReadOnlyList<AccountResponse>> Handle(GetAllAccountsQuery request, CancellationToken cancellationToken) =>
        (await repository.GetAllAsync(cancellationToken)).ToResponse();
}
