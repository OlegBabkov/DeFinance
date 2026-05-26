using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.MandatoryPayment;
using MediatR;

namespace DeFinance.Application.MandatoryPayments.Queries;

public record GetMandatoryPaymentByIdQuery(Guid Id) : IRequest<MandatoryPaymentResponse?>;

public class GetMandatoryPaymentByIdQueryHandler(IMandatoryPaymentRepository repository)
    : IRequestHandler<GetMandatoryPaymentByIdQuery, MandatoryPaymentResponse?>
{
    public async Task<MandatoryPaymentResponse?> Handle(
        GetMandatoryPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await repository.GetByIdAsync(request.Id, cancellationToken);
        return payment?.ToResponse();
    }
}
