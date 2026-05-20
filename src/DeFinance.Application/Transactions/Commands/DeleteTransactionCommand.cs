using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Domain.Entities;
using MediatR;

namespace DeFinance.Application.Transactions.Commands;

public record DeleteTransactionCommand(Guid Id) : IRequest<bool>;

public class DeleteTransactionCommandHandler(ITransactionRepository transactionRepository)
    : IRequestHandler<DeleteTransactionCommand, bool>
{
    public async Task<bool> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await transactionRepository.GetByIdAsync(request.Id, cancellationToken);
        if (transaction is null) return false;

        var account  = transaction.Account!;
        var category = transaction.Category!;

        account.AdjustBalance(-BalanceDelta(category.Type, transaction.Sum));

        transactionRepository.Remove(transaction);
        await transactionRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static decimal BalanceDelta(CategoryType type, decimal sum) => type switch
    {
        CategoryType.Income  =>  sum,
        CategoryType.Expense => -sum,
        _                    =>  0m,
    };
}
