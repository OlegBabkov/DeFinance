using DeFinance.Application.Abstractions.Repositories;
using MediatR;

namespace DeFinance.Application.MandatoryPayments.Commands;

public record UpdateMandatoryPaymentStatusCommand(
    Guid Id,
    Guid? PaymentStatusId
) : IRequest<bool>;

public class UpdateMandatoryPaymentStatusCommandHandler(IMandatoryPaymentRepository repository)
    : IRequestHandler<UpdateMandatoryPaymentStatusCommand, bool>
{
    public async Task<bool> Handle(UpdateMandatoryPaymentStatusCommand request, CancellationToken cancellationToken)
    {
        var payment = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (payment is null) return false;

        payment.UpdatePaymentStatus(request.PaymentStatusId);
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
