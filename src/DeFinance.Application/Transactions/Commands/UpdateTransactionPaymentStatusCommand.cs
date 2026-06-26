using DeFinance.Application.Abstractions.Repositories;
using MediatR;

namespace DeFinance.Application.Transactions.Commands;

public record UpdateTransactionPaymentStatusCommand(
    Guid TransactionId,
    Guid PaymentStatusId
) : IRequest<bool>;

public class UpdateTransactionPaymentStatusCommandHandler(ITransactionRepository transactionRepository)
    : IRequestHandler<UpdateTransactionPaymentStatusCommand, bool>
{
    public async Task<bool> Handle(UpdateTransactionPaymentStatusCommand request, CancellationToken cancellationToken)
    {
        var transaction = await transactionRepository.GetByIdAsync(request.TransactionId, cancellationToken);
        if (transaction is null) return false;

        transaction.UpdatePaymentStatus(request.PaymentStatusId);
        await transactionRepository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
