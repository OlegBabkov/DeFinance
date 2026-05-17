using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.Account;
using MediatR;

namespace DeFinance.Application.Accounts.Queries;

public record GetAccountByIdQuery(Guid Id) : IRequest<AccountResponse?>;

public class GetAccountByIdQueryHandler(IAccountRepository repository)
    : IRequestHandler<GetAccountByIdQuery, AccountResponse?>
{
    public async Task<AccountResponse?> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var account = await repository.GetByIdAsync(request.Id, cancellationToken);
        return account?.ToResponse();
    }
}
