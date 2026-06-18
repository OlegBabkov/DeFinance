using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.Transaction;
using DeFinance.Domain.Entities;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.Transactions.Commands;

public record CreateTransactionCommand(
    DateTime DateTime,
    decimal Sum,
    decimal ExchangeRate,
    Guid InCurrencyId,
    Guid AccountId,
    Guid CategoryId,
    Guid? CounterpartyId,
    Guid PaymentStatusId,
    string? Notes
) : IRequest<TransactionResponse>;

public class CreateTransactionCommandHandler(
    ITransactionRepository transactionRepository,
    IAccountRepository accountRepository,
    ICategoryRepository categoryRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<CreateTransactionCommand, TransactionResponse>
{
    public async Task<TransactionResponse> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var account  = await accountRepository.GetByIdAsync(request.AccountId, cancellationToken)
            ?? throw new InvalidOperationException($"Account {request.AccountId} not found.");
        var category = await categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new InvalidOperationException($"Category {request.CategoryId} not found.");

        var transaction = Transaction.Create(
            request.DateTime, request.Sum, request.ExchangeRate,
            request.InCurrencyId, request.AccountId, request.CategoryId,
            request.CounterpartyId, request.PaymentStatusId, currentUserService.UserId, request.Notes);

        account.AdjustBalance(BalanceDelta(category.Type, request.Sum));

        await transactionRepository.AddAsync(transaction, cancellationToken);
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

public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
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
