using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.Transaction;
using DeFinance.Domain.Entities;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.Transactions.Commands;

public record UpdateTransactionCommand(
    Guid Id,
    DateTime DateTime,
    decimal Sum,
    decimal ExchangeRate,
    Guid InCurrencyId,
    Guid AccountId,
    Guid CategoryId,
    Guid? CounterpartyId,
    Guid PaymentStatusId,
    string? Notes
) : IRequest<TransactionResponse?>;

public class UpdateTransactionCommandHandler(
    ITransactionRepository transactionRepository,
    IAccountRepository accountRepository,
    ICategoryRepository categoryRepository)
    : IRequestHandler<UpdateTransactionCommand, TransactionResponse?>
{
    public async Task<TransactionResponse?> Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await transactionRepository.GetByIdAsync(request.Id, cancellationToken);
        if (transaction is null) return null;

        var oldAccount  = transaction.Account!;
        var oldCategory = transaction.Category!;

        // reverse old balance effect
        oldAccount.AdjustBalance(-BalanceDelta(oldCategory.Type, transaction.Sum));

        // resolve new account (may differ)
        var newAccount = transaction.AccountId == request.AccountId
            ? oldAccount
            : await accountRepository.GetByIdAsync(request.AccountId, cancellationToken)
                ?? throw new InvalidOperationException($"Account {request.AccountId} not found.");

        // resolve new category (may differ)
        var newCategory = transaction.CategoryId == request.CategoryId
            ? oldCategory
            : await categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken)
                ?? throw new InvalidOperationException($"Category {request.CategoryId} not found.");

        // apply new balance effect
        newAccount.AdjustBalance(BalanceDelta(newCategory.Type, request.Sum));

        transaction.Update(
            request.DateTime, request.Sum, request.ExchangeRate,
            request.InCurrencyId, request.AccountId, request.CategoryId,
            request.CounterpartyId, request.PaymentStatusId, request.Notes);

        await transactionRepository.SaveChangesAsync(cancellationToken);

        return (await transactionRepository.GetByIdAsync(transaction.Id, cancellationToken))!.ToResponse();
    }

    private static decimal BalanceDelta(CategoryType type, decimal sum) => type switch
    {
        CategoryType.Income      =>  sum,
        CategoryType.Expense     => -sum,
        CategoryType.TransferIn  =>  sum,
        CategoryType.TransferOut => -sum,
        _                        =>  0m,
    };
}

public class UpdateTransactionCommandValidator : AbstractValidator<UpdateTransactionCommand>
{
    public UpdateTransactionCommandValidator()
    {
        RuleFor(x => x.Sum).GreaterThan(0).WithMessage("Sum must be greater than zero.");
        RuleFor(x => x.ExchangeRate).GreaterThan(0).WithMessage("Exchange rate must be greater than zero.");
        RuleFor(x => x.InCurrencyId).NotEmpty().WithMessage("Reporting currency is required.");
        RuleFor(x => x.AccountId).NotEmpty().WithMessage("Account is required.");
        RuleFor(x => x.CategoryId).NotEmpty().WithMessage("Category is required.");
        RuleFor(x => x.PaymentStatusId).NotEmpty().WithMessage("Payment status is required.");
        RuleFor(x => x.Notes).MaximumLength(500).When(x => x.Notes is not null);
    }
}
