using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.PaymentStatus;
using MediatR;

namespace DeFinance.Application.PaymentStatuses.Commands;

public record ActivatePaymentStatusCommand(Guid Id) : IRequest<PaymentStatusResponse?>;

public class ActivatePaymentStatusCommandHandler(IPaymentStatusRepository repository)
    : IRequestHandler<ActivatePaymentStatusCommand, PaymentStatusResponse?>
{
    public async Task<PaymentStatusResponse?> Handle(ActivatePaymentStatusCommand request, CancellationToken cancellationToken)
    {
        var status = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (status is null) return null;
        status.Activate();
        await repository.SaveChangesAsync(cancellationToken);
        return status.ToResponse();
    }
}
