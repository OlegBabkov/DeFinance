using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.MandatoryPayment;
using MediatR;

namespace DeFinance.Application.MandatoryPayments.Commands;

public record ActivateMandatoryPaymentCommand(Guid Id) : IRequest<MandatoryPaymentResponse?>;

public class ActivateMandatoryPaymentCommandHandler(IMandatoryPaymentRepository repository)
    : IRequestHandler<ActivateMandatoryPaymentCommand, MandatoryPaymentResponse?>
{
    public async Task<MandatoryPaymentResponse?> Handle(
        ActivateMandatoryPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (payment is null) return null;

        payment.Activate();
        await repository.SaveChangesAsync(cancellationToken);
        return payment.ToResponse();
    }
}
