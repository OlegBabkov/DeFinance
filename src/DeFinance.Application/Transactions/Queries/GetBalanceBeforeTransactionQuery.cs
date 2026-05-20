using DeFinance.Application.Abstractions.Repositories;
using MediatR;

namespace DeFinance.Application.Transactions.Queries;

public record GetBalanceBeforeTransactionQuery(Guid TransactionId) : IRequest<decimal?>;

public class GetBalanceBeforeTransactionQueryHandler(ITransactionRepository repository)
    : IRequestHandler<GetBalanceBeforeTransactionQuery, decimal?>
{
    public Task<decimal?> Handle(GetBalanceBeforeTransactionQuery request, CancellationToken cancellationToken) =>
        repository.GetBalanceBeforeAsync(request.TransactionId, cancellationToken);
}
