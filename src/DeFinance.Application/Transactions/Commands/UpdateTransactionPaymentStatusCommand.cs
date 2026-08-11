using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Domain.Entities;
using MediatR;

namespace DeFinance.Application.Transactions.Commands;

public record UpdateTransactionPaymentStatusCommand(
    Guid TransactionId,
    Guid PaymentStatusId
) : IRequest<bool>;

public class UpdateTransactionPaymentStatusCommandHandler(
    ITransactionRepository transactionRepository,
    IPaymentStatusRepository paymentStatusRepository)
    : IRequestHandler<UpdateTransactionPaymentStatusCommand, bool>
{
    public async Task<bool> Handle(UpdateTransactionPaymentStatusCommand request, CancellationToken cancellationToken)
    {
        var transaction = await transactionRepository.GetByIdAsync(request.TransactionId, cancellationToken);
        if (transaction is null) return false;

        var oldStatus = transaction.PaymentStatus!;
        var newStatus = await paymentStatusRepository.GetByIdAsync(request.PaymentStatusId, cancellationToken)
            ?? throw new InvalidOperationException($"PaymentStatus {request.PaymentStatusId} not found.");

        // When switching between affecting/not-affecting status, adjust the account balance
        if (oldStatus.AffectsBalance != newStatus.AffectsBalance)
        {
            var delta = BalanceDelta(transaction.Category!.Type, transaction.Sum);
            transaction.Account!.AdjustBalance(newStatus.AffectsBalance ? delta : -delta);
        }

        transaction.UpdatePaymentStatus(request.PaymentStatusId);
        await transactionRepository.SaveChangesAsync(cancellationToken);
        return true;
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
