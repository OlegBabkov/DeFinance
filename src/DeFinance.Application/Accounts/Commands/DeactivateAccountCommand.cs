using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.Account;
using MediatR;

namespace DeFinance.Application.Accounts.Commands;

public record DeactivateAccountCommand(Guid Id) : IRequest<AccountResponse?>;

public class DeactivateAccountCommandHandler(IAccountRepository repository)
    : IRequestHandler<DeactivateAccountCommand, AccountResponse?>
{
    public async Task<AccountResponse?> Handle(DeactivateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (account is null) return null;

        account.Deactivate();
        await repository.SaveChangesAsync(cancellationToken);
        return account.ToResponse();
    }
}
