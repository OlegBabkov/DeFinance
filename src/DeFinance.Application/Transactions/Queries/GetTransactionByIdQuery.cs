using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.Transaction;
using MediatR;

namespace DeFinance.Application.Transactions.Queries;

public record GetTransactionByIdQuery(Guid Id) : IRequest<TransactionResponse?>;

public class GetTransactionByIdQueryHandler(ITransactionRepository repository)
    : IRequestHandler<GetTransactionByIdQuery, TransactionResponse?>
{
    public async Task<TransactionResponse?> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        var transaction = await repository.GetByIdAsync(request.Id, cancellationToken);
        return transaction?.ToResponse();
    }
}
