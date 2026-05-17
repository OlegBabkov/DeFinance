using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.Account;
using MediatR;

namespace DeFinance.Application.Accounts.Commands;

public record ActivateAccountCommand(Guid Id) : IRequest<AccountResponse?>;

public class ActivateAccountCommandHandler(IAccountRepository repository)
    : IRequestHandler<ActivateAccountCommand, AccountResponse?>
{
    public async Task<AccountResponse?> Handle(ActivateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (account is null) return null;

        account.Activate();
        await repository.SaveChangesAsync(cancellationToken);
        return account.ToResponse();
    }
}
