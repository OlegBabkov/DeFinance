using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.Account;
using DeFinance.Domain.Entities;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.Accounts.Commands;

public record CreateAccountCommand(string Name, AccountType Type, decimal InitialBalance, Guid CurrencyId)
    : IRequest<AccountResponse>;

public class CreateAccountCommandHandler(IAccountRepository repository, ICurrentUserService currentUserService)
    : IRequestHandler<CreateAccountCommand, AccountResponse>
{
    public async Task<AccountResponse> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = Account.Create(request.Name, request.Type, request.InitialBalance, request.CurrencyId, currentUserService.UserId!.Value);
        await repository.AddAsync(account, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return account.ToResponse();
    }
}

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Account name is required.")
            .MaximumLength(100).WithMessage("Account name must not exceed 100 characters.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Account type is invalid.");

        RuleFor(x => x.InitialBalance)
            .GreaterThanOrEqualTo(0).WithMessage("Initial balance must be zero or positive.");

        RuleFor(x => x.CurrencyId)
            .NotEmpty().WithMessage("Currency is required.");
    }
}
