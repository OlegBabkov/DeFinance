using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.PaymentStatus;
using MediatR;

namespace DeFinance.Application.PaymentStatuses.Commands;

public record DeactivatePaymentStatusCommand(Guid Id) : IRequest<PaymentStatusResponse?>;

public class DeactivatePaymentStatusCommandHandler(IPaymentStatusRepository repository)
    : IRequestHandler<DeactivatePaymentStatusCommand, PaymentStatusResponse?>
{
    public async Task<PaymentStatusResponse?> Handle(DeactivatePaymentStatusCommand request, CancellationToken cancellationToken)
    {
        var status = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (status is null) return null;
        status.Deactivate();
        await repository.SaveChangesAsync(cancellationToken);
        return status.ToResponse();
    }
}
