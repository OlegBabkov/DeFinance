using DeFinance.Application.Abstractions.Repositories;
using MediatR;

namespace DeFinance.Application.MandatoryPayments.Commands;

public record ResetMandatoryPaymentStatusesCommand(Guid AccountId) : IRequest<int>;

public class ResetMandatoryPaymentStatusesCommandHandler(IMandatoryPaymentRepository repository)
    : IRequestHandler<ResetMandatoryPaymentStatusesCommand, int>
{
    public async Task<int> Handle(
        ResetMandatoryPaymentStatusesCommand request, CancellationToken cancellationToken) =>
        await repository.ResetPaymentStatusesByAccountAsync(request.AccountId, cancellationToken);
}
