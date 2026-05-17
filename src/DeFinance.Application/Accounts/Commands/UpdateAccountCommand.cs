using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.Account;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.Accounts.Commands;

public record UpdateAccountCommand(Guid Id, string Name) : IRequest<AccountResponse?>;

public class UpdateAccountCommandHandler(IAccountRepository repository)
    : IRequestHandler<UpdateAccountCommand, AccountResponse?>
{
    public async Task<AccountResponse?> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (account is null) return null;

        account.Update(request.Name);
        await repository.SaveChangesAsync(cancellationToken);
        return account.ToResponse();
    }
}

public class UpdateAccountCommandValidator : AbstractValidator<UpdateAccountCommand>
{
    public UpdateAccountCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Account name is required.")
            .MaximumLength(100).WithMessage("Account name must not exceed 100 characters.");
    }
}
