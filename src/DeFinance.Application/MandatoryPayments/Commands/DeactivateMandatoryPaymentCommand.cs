using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.MandatoryPayment;
using MediatR;

namespace DeFinance.Application.MandatoryPayments.Commands;

public record DeactivateMandatoryPaymentCommand(Guid Id) : IRequest<MandatoryPaymentResponse?>;

public class DeactivateMandatoryPaymentCommandHandler(IMandatoryPaymentRepository repository)
    : IRequestHandler<DeactivateMandatoryPaymentCommand, MandatoryPaymentResponse?>
{
    public async Task<MandatoryPaymentResponse?> Handle(
        DeactivateMandatoryPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (payment is null) return null;

        payment.Deactivate();
        await repository.SaveChangesAsync(cancellationToken);
        return payment.ToResponse();
    }
}
