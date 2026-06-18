using DeFinance.Application.Abstractions.Repositories;
using MediatR;

namespace DeFinance.Application.Accounts.Commands;

public record ReorderAccountsCommand(IReadOnlyList<Guid> OrderedIds) : IRequest;

public class ReorderAccountsCommandHandler(IAccountRepository repository)
    : IRequestHandler<ReorderAccountsCommand>
{
    public async Task Handle(ReorderAccountsCommand request, CancellationToken cancellationToken)
    {
        await repository.ReorderAsync(request.OrderedIds, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
